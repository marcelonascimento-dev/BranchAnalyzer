using System.Text.Json;
using BranchAnalyzer.Api;
using BranchAnalyzer.Api.Models;
using BranchAnalyzer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Parse port from args (default 5391)
var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5391;

builder.Services.AddSingleton<GitService>();
builder.Services.AddSingleton(AppSettings.Load());
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.WebHost.UseUrls($"http://localhost:{port}");

var app = builder.Build();
app.UseCors();

var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

// ══════════════════════════════════════════════════════════════
//  Health
// ══════════════════════════════════════════════════════════════

app.MapGet("/", () => Results.Ok(new { status = "ok", timestamp = DateTime.Now }));

// ══════════════════════════════════════════════════════════════
//  Repo
// ══════════════════════════════════════════════════════════════

app.MapPost("/api/repo/set", (SetRepoRequest req, GitService git, AppSettings settings) =>
{
    if (!Directory.Exists(Path.Combine(req.Path, ".git")))
        return Results.BadRequest(new { error = "Not a git repository" });

    git.RepoPath = req.Path;
    settings.AddRecentRepo(req.Path);
    settings.Save();
    Logger.Info($"Repo set: {req.Path}");
    return Results.Ok(new { path = req.Path });
});

app.MapPost("/api/repo/validate-url", (ValidateUrlRequest req) =>
{
    var (ok, error) = GitService.ValidateGitUrl(req.Url);
    return Results.Ok(new { ok, error });
});

app.MapGet("/api/repo/settings", (AppSettings settings) => Results.Ok(settings));

app.MapPost("/api/repo/settings", async (HttpRequest httpReq, AppSettings settings) =>
{
    var updated = await httpReq.ReadFromJsonAsync<AppSettings>();
    if (updated == null) return Results.BadRequest();

    settings.LastRepoPath = updated.LastRepoPath;
    settings.LastRepoUrl = updated.LastRepoUrl;
    settings.LastBranchA = updated.LastBranchA;
    settings.LastBranchB = updated.LastBranchB;
    settings.LastBatchReceptor = updated.LastBatchReceptor;
    settings.LastSelectedTab = updated.LastSelectedTab;
    settings.FetchOnOpen = updated.FetchOnOpen;
    settings.CloneCachePath = updated.CloneCachePath;
    settings.WindowWidth = updated.WindowWidth;
    settings.WindowHeight = updated.WindowHeight;
    settings.WindowX = updated.WindowX;
    settings.WindowY = updated.WindowY;
    settings.WindowMaximized = updated.WindowMaximized;
    settings.Save();
    return Results.Ok(settings);
});

// Clone with SSE progress
app.MapPost("/api/repo/clone", async (CloneRepoRequest req, HttpResponse response, GitService git, AppSettings settings) =>
{
    response.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";

    await response.WriteAsync($"data: {{\"type\":\"progress\",\"message\":\"Validating URL...\"}}\n\n");
    await response.Body.FlushAsync();

    var (ok, validateError) = GitService.ValidateGitUrl(req.Url);
    if (!ok)
    {
        await response.WriteAsync($"data: {{\"type\":\"error\",\"message\":\"{EscapeJson(validateError)}\"}}\n\n");
        return;
    }

    var (path, cloneError) = GitService.CloneRepository(req.Url, progress =>
    {
        var msg = $"data: {{\"type\":\"progress\",\"message\":\"{EscapeJson(progress)}\"}}\n\n";
        response.WriteAsync(msg).Wait();
        response.Body.FlushAsync().Wait();
    }, req.CachePath);

    if (!string.IsNullOrEmpty(cloneError))
    {
        await response.WriteAsync($"data: {{\"type\":\"error\",\"message\":\"{EscapeJson(cloneError)}\"}}\n\n");
        return;
    }

    git.RepoPath = path;
    settings.LastRepoPath = path;
    settings.LastRepoUrl = req.Url;
    settings.AddRecentRepo(path);
    settings.Save();

    await response.WriteAsync($"data: {{\"type\":\"done\",\"path\":\"{EscapeJson(path)}\"}}\n\n");
});

// ══════════════════════════════════════════════════════════════
//  Branches
// ══════════════════════════════════════════════════════════════

app.MapGet("/api/branches", (GitService git) =>
{
    var prioritized = git.GetBranchesPrioritized();
    var local = git.GetLocalBranches();
    return Results.Ok(new { prioritized, local });
});

app.MapGet("/api/branches/metadata", (GitService git) =>
    Results.Ok(git.GetBranchesMetadata()));

app.MapGet("/api/branches/current", (GitService git) =>
{
    try { return Results.Ok(new { branch = git.GetCurrentBranch() }); }
    catch { return Results.Ok(new { branch = "(unknown)" }); }
});

app.MapGet("/api/branches/resolve", (string name, GitService git) =>
{
    var resolved = git.ResolveBranch(name);
    return resolved != null
        ? Results.Ok(new { resolved })
        : Results.NotFound(new { error = $"Branch '{name}' not found" });
});

app.MapPost("/api/branches/fetch", (GitService git) =>
{
    git.FetchOrigin();
    return Results.Ok(new { message = "Fetch complete" });
});

app.MapPost("/api/branches/fetch-prune", (GitService git) =>
{
    git.FetchPrune();
    return Results.Ok(new { message = "Fetch + prune complete" });
});

app.MapGet("/api/branches/local-info", (GitService git) =>
    Results.Ok(git.GetLocalBranchesInfo()));

app.MapGet("/api/branches/ahead-behind", (GitService git) =>
{
    var (ahead, behind) = git.GetAheadBehind();
    return Results.Ok(new { ahead, behind });
});

// ══════════════════════════════════════════════════════════════
//  Merge Analysis
// ══════════════════════════════════════════════════════════════

app.MapGet("/api/merge/status", (string a, string b, GitService git) =>
    Results.Ok(git.CheckMergeStatus(a, b)));

app.MapGet("/api/merge/pending-commits", (string a, string b, GitService git) =>
    Results.Ok(git.GetPendingCommits(a, b)));

app.MapGet("/api/merge/changed-files", (string a, string b, GitService git) =>
    Results.Ok(git.GetChangedFiles(a, b)));

app.MapGet("/api/merge/conflicts", (string a, string b, GitService git) =>
    Results.Ok(git.DetectPotentialConflicts(a, b)));

app.MapGet("/api/merge/branch-info", (string a, string b, GitService git) =>
    Results.Ok(git.GetBranchInfo(a, b)));

app.MapGet("/api/merge/contributors", (string a, string b, GitService git) =>
    Results.Ok(git.GetContributors(a, b)));

app.MapGet("/api/merge/stats", (string a, string b, GitService git) =>
    Results.Ok(git.GetDiffStats(a, b)));

app.MapGet("/api/merge/large-commits", (string a, string b, GitService git) =>
    Results.Ok(git.GetLargeCommits(a, b)));

// ══════════════════════════════════════════════════════════════
//  Batch Analysis (SSE)
// ══════════════════════════════════════════════════════════════

app.MapPost("/api/batch/analyze", async (BatchAnalyzeRequest req, HttpResponse response, GitService git) =>
{
    response.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";

    var receptor = git.ResolveBranch(req.Receptor);
    if (receptor == null)
    {
        await response.WriteAsync($"data: {{\"type\":\"error\",\"message\":\"Receptor branch not found\"}}\n\n");
        return;
    }

    var total = req.Branches.Count;
    var completed = 0;

    var semaphore = new SemaphoreSlim(4);
    var lockObj = new object();

    var tasks = req.Branches.Select(async branchName =>
    {
        await semaphore.WaitAsync();
        try
        {
            var resolved = git.ResolveBranch(branchName);
            BatchMergeResult result;

            if (resolved == null)
            {
                result = new BatchMergeResult
                {
                    BranchFeature = branchName,
                    Status = "NAO ENCONTRADO"
                };
            }
            else
            {
                var status = git.CheckMergeStatus(receptor, resolved);
                var conflicts = git.DetectPotentialConflicts(receptor, resolved);
                var files = git.GetChangedFiles(receptor, resolved);
                var info = git.GetBranchInfo(receptor, resolved);

                result = new BatchMergeResult
                {
                    BranchFeature = branchName,
                    IsMerged = status.IsMerged,
                    Status = status.IsMerged ? "MERGED" : "PENDENTE",
                    CommitsPendentes = status.PendingCommits,
                    ConflitosArquivos = conflicts.Count,
                    ArquivosAlterados = files.Count,
                    UltimoAutor = info.LastCommitAuthor,
                    UltimoCommit = info.LastCommitMessage
                };
            }

            int current;
            lock (lockObj) { current = ++completed; }

            var json = JsonSerializer.Serialize(result, jsonOpts);
            var msg = $"data: {{\"type\":\"result\",\"current\":{current},\"total\":{total},\"result\":{json}}}\n\n";

            lock (lockObj)
            {
                response.WriteAsync(msg).Wait();
                response.Body.FlushAsync().Wait();
            }
        }
        catch (Exception ex)
        {
            int current;
            lock (lockObj) { current = ++completed; }

            var errorResult = new BatchMergeResult
            {
                BranchFeature = branchName,
                Status = "ERRO",
                UltimoCommit = ex.Message
            };
            var json = JsonSerializer.Serialize(errorResult, jsonOpts);
            lock (lockObj)
            {
                response.WriteAsync($"data: {{\"type\":\"result\",\"current\":{current},\"total\":{total},\"result\":{json}}}\n\n").Wait();
                response.Body.FlushAsync().Wait();
            }
        }
        finally
        {
            semaphore.Release();
        }
    });

    await Task.WhenAll(tasks);
    await response.WriteAsync("data: {\"type\":\"done\"}\n\n");
});

// ══════════════════════════════════════════════════════════════
//  My Branch (composite)
// ══════════════════════════════════════════════════════════════

app.MapGet("/api/mybranch/info", (GitService git) =>
{
    var branch = git.GetCurrentBranch();
    var (ahead, behind) = git.GetAheadBehind();
    var recentCommits = git.GetRecentCommits(30);
    var localChanges = git.GetLocalChanges();
    var stashes = git.GetStashes();
    var localBranches = git.GetLocalBranchesInfo();

    return Results.Ok(new
    {
        branch,
        ahead,
        behind,
        recentCommits,
        localChanges,
        stashes,
        localBranches
    });
});

// ══════════════════════════════════════════════════════════════
//  Branch Health
// ══════════════════════════════════════════════════════════════

app.MapGet("/api/health/branches", (GitService git) =>
{
    var metadata = git.GetBranchesMetadata();
    var now = DateTime.Now;

    var branches = metadata.Select(bm =>
    {
        var daysInactive = (int)(now - bm.Date).TotalDays;
        var status = daysInactive > 180 ? "OBSOLETO" : daysInactive > 60 ? "INATIVO" : "ATIVO";
        return new
        {
            bm.ShortName,
            bm.FullName,
            bm.DateShort,
            bm.Date,
            bm.Author,
            bm.Prefix,
            daysInactive,
            status
        };
    })
    .OrderByDescending(b => b.daysInactive)
    .ToList();

    var active = branches.Count(b => b.status == "ATIVO");
    var stale = branches.Count(b => b.status == "INATIVO");
    var obsolete = branches.Count(b => b.status == "OBSOLETO");

    return Results.Ok(new { total = branches.Count, active, stale, obsolete, branches });
});

// ══════════════════════════════════════════════════════════════

Logger.Info($"BranchAnalyzer API starting on port {port}");
app.Run();

static string EscapeJson(string s) =>
    s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
