using BranchAnalyzer.Api.Models;
using System.Diagnostics;
using System.Text;

namespace BranchAnalyzer.Api.Services;

/// <summary>
/// Serviço que executa comandos Git somente leitura.
/// NUNCA altera o repositório.
/// </summary>
public class GitService
{
    public string RepoPath { get; set; } = "";

    public GitService() { }
    public GitService(string repoPath) => RepoPath = repoPath;

    private (string stdout, string stderr, int exitCode) RunGit(params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = RepoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit(60_000);
        return (stdout.Trim(), stderr.Trim(), proc.ExitCode);
    }

    private string Git(params string[] args)
    {
        var (stdout, _, _) = RunGit(args);
        return stdout;
    }

    public string GetCurrentBranch()
    {
        var branch = Git("rev-parse", "--abbrev-ref", "HEAD");
        return string.IsNullOrEmpty(branch) ? "(desconhecido)" : branch.Trim();
    }

    // ── Resolução de branches ──────────────────────────────────────

    public string? ResolveBranch(string name)
    {
        // Sempre forçar versão remota (origin/)
        var remoteName = name.StartsWith("origin/") ? name : $"origin/{name}";
        var (o, _, code) = RunGit("rev-parse", "--verify", remoteName);
        if (code == 0 && !string.IsNullOrEmpty(o)) return remoteName;

        // Fallback: tentar exatamente como digitado
        (o, _, code) = RunGit("rev-parse", "--verify", name);
        if (code == 0 && !string.IsNullOrEmpty(o)) return name;

        return null;
    }

    /// <summary>Retorna branches locais (os que o dev trabalhou recentemente)</summary>
    public List<string> GetLocalBranches()
    {
        var local = Git("branch", "--sort=-committerdate", "--format=%(refname:short)");
        var result = new List<string>();
        foreach (var b in local.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var name = b.Trim();
            if (!string.IsNullOrEmpty(name))
                result.Add(name);
        }
        return result;
    }

    /// <summary>Retorna todos os branches remotos sem prefixo origin/</summary>
    public List<string> GetAllBranches()
    {
        var remote = Git("branch", "-r", "--format=%(refname:short)");
        var all = new HashSet<string>();
        foreach (var b in remote.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var name = b.Trim();
            if (name.Contains("->")) continue;
            var shortName = name.StartsWith("origin/") ? name["origin/".Length..] : name;
            if (!string.IsNullOrEmpty(shortName))
                all.Add(shortName);
        }
        return all.OrderBy(x => x).ToList();
    }

    /// <summary>Branches ordenados: locais primeiro (recentes), depois remotos. Sem duplicatas.</summary>
    public List<string> GetBranchesPrioritized()
    {
        var localBranches = GetLocalBranches();
        var allRemote = GetAllBranches();

        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Primeiro: branches locais (ordenados por ultimo uso)
        foreach (var b in localBranches)
        {
            if (seen.Add(b))
                result.Add(b);
        }

        // Depois: branches remotos que nao existem localmente
        foreach (var b in allRemote)
        {
            if (seen.Add(b))
                result.Add(b);
        }

        return result;
    }

    public void FetchPrune()
    {
        RunGit("fetch", "--prune", "--no-tags");
    }

    /// <summary>Fetch leve igual ao Visual Studio (git fetch origin)</summary>
    public void FetchOrigin()
    {
        RunGit("fetch", "origin");
    }

    // ── Merge base ─────────────────────────────────────────────────

    public string GetMergeBase(string a, string b) => Git("merge-base", a, b);

    // ── Status de merge ────────────────────────────────────────────

    public MergeStatus CheckMergeStatus(string branchA, string branchB)
    {
        var pending = Git("log", "--oneline", $"{branchA}..{branchB}");
        var ahead = Git("log", "--oneline", $"{branchB}..{branchA}");
        var mergeBase = GetMergeBase(branchA, branchB);
        var tipB = Git("rev-parse", branchB);

        var pendingCount = string.IsNullOrEmpty(pending) ? 0 : pending.Split('\n').Length;
        var aheadCount = string.IsNullOrEmpty(ahead) ? 0 : ahead.Split('\n').Length;
        var isMerged = pendingCount == 0 || mergeBase == tipB;

        return new MergeStatus
        {
            IsMerged = isMerged,
            PendingCommits = pendingCount,
            AheadCommits = aheadCount,
            MergeBase = mergeBase
        };
    }

    // ── Commits pendentes ──────────────────────────────────────────

    public List<CommitInfo> GetPendingCommits(string branchA, string branchB)
    {
        var log = Git("log", "--format=%h|%an|%ar|%ai|%s", $"{branchA}..{branchB}");
        return ParseCommits(log);
    }

    // ── Arquivos alterados ─────────────────────────────────────────

    public List<FileChange> GetChangedFiles(string branchA, string branchB)
    {
        var diff = Git("diff", "--name-status", $"{branchA}...{branchB}");
        var result = new List<FileChange>();
        foreach (var line in diff.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t', 2);
            if (parts.Length < 2) continue;
            result.Add(new FileChange
            {
                Status = parts[0].Trim() switch
                {
                    "A" => "Adicionado",
                    "M" => "Modificado",
                    "D" => "Removido",
                    var s when s.StartsWith("R") => "Renomeado",
                    _ => parts[0].Trim()
                },
                StatusCode = parts[0].Trim()[0],
                FilePath = parts[1].Trim()
            });
        }
        return result;
    }

    // ── Conflitos potenciais ───────────────────────────────────────

    public List<string> DetectPotentialConflicts(string branchA, string branchB)
    {
        var mergeBase = GetMergeBase(branchA, branchB);
        if (string.IsNullOrEmpty(mergeBase)) return new();

        var filesA = Git("diff", "--name-only", $"{mergeBase}..{branchA}")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var filesB = Git("diff", "--name-only", $"{mergeBase}..{branchB}")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        return filesA.Intersect(filesB).OrderBy(x => x).ToList();
    }

    // ── Contribuidores ─────────────────────────────────────────────

    public List<ContributorInfo> GetContributors(string branchA, string branchB)
    {
        var log = Git("log", "--format=%an|%ae", $"{branchA}..{branchB}");
        var dict = new Dictionary<string, ContributorInfo>();
        foreach (var line in log.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 2);
            if (parts.Length < 2) continue;
            var key = $"{parts[0].Trim()} <{parts[1].Trim()}>";
            if (!dict.ContainsKey(key))
                dict[key] = new ContributorInfo { Name = parts[0].Trim(), Email = parts[1].Trim() };
            dict[key].CommitCount++;
        }
        return dict.Values.OrderByDescending(x => x.CommitCount).ToList();
    }

    // ── Timeline ───────────────────────────────────────────────────

    public Dictionary<string, List<CommitInfo>> GetTimeline(string branchA, string branchB)
    {
        var log = Git("log", "--format=%h|%an|%ar|%ai|%s", $"{branchA}..{branchB}");
        var commits = ParseCommits(log);
        return commits.GroupBy(c => c.Date.Date.ToString("yyyy-MM-dd"))
                      .OrderByDescending(g => g.Key)
                      .ToDictionary(g => g.Key, g => g.ToList());
    }

    // ── Estatísticas ───────────────────────────────────────────────

    public DiffStats GetDiffStats(string branchA, string branchB)
    {
        var stat = Git("diff", "--shortstat", $"{branchA}...{branchB}");
        var files = Git("diff", "--name-only", $"{branchA}...{branchB}");

        var extCount = new Dictionary<string, int>();
        foreach (var f in files.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var ext = Path.GetExtension(f);
            if (string.IsNullOrEmpty(ext)) ext = "(sem extensão)";
            extCount[ext] = extCount.GetValueOrDefault(ext) + 1;
        }

        return new DiffStats
        {
            Summary = stat,
            FilesByExtension = extCount.OrderByDescending(x => x.Value)
                                       .ToDictionary(x => x.Key, x => x.Value)
        };
    }

    // ── Info do branch ─────────────────────────────────────────────

    public BranchInfo GetBranchInfo(string branchA, string branchB)
    {
        var mergeBase = GetMergeBase(branchA, branchB);
        var baseDate = Git("log", "-1", "--format=%ai", mergeBase);
        var lastCommit = Git("log", "-1", "--format=%ai|%an|%s", branchB);
        var firstCommitLog = Git("log", "--reverse", "--format=%ai", $"{branchA}..{branchB}");
        var firstLine = firstCommitLog.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

        var info = new BranchInfo { DivergenceDate = baseDate.Length >= 10 ? baseDate[..10] : "" };

        if (lastCommit.Contains('|'))
        {
            var p = lastCommit.Split('|', 3);
            info.LastCommitDate = p[0].Length >= 10 ? p[0][..10] : "";
            info.LastCommitAuthor = p.Length > 1 ? p[1] : "";
            info.LastCommitMessage = p.Length > 2 ? p[2] : "";
        }

        info.FirstCommitDate = firstLine.Length >= 10 ? firstLine[..10] : "";
        return info;
    }

    // ── Commits grandes ────────────────────────────────────────────

    public List<LargeCommit> GetLargeCommits(string branchA, string branchB, int top = 10)
    {
        var log = Git("log", "--format=%h", $"{branchA}..{branchB}");
        var result = new List<LargeCommit>();

        foreach (var h in log.Split('\n', StringSplitOptions.RemoveEmptyEntries).Take(200))
        {
            var stat = Git("diff", "--shortstat", $"{h}^..{h}");
            if (string.IsNullOrEmpty(stat)) continue;

            int changes = 0;
            foreach (var part in stat.Split(','))
            {
                if (part.Contains("insertion") || part.Contains("deletion"))
                {
                    var numStr = new string(part.Trim().TakeWhile(char.IsDigit).ToArray());
                    if (int.TryParse(numStr, out var n)) changes += n;
                }
            }

            var msg = Git("log", "-1", "--format=%an|%s", h);
            var parts = msg.Split('|', 2);
            result.Add(new LargeCommit
            {
                Hash = h,
                LinesChanged = changes,
                Author = parts.Length > 0 ? parts[0] : "",
                Message = parts.Length > 1 ? parts[1] : ""
            });
        }

        return result.OrderByDescending(x => x.LinesChanged).Take(top).ToList();
    }

    // ── Busca por keyword ──────────────────────────────────────────

    public List<CommitInfo> SearchCommits(string branchA, string branchB, string keyword)
    {
        var log = Git("log", $"--grep={keyword}", "-i", "--format=%h|%an|%ar|%ai|%s", $"{branchA}..{branchB}");
        return ParseCommits(log);
    }

    // ── Branches remotos ───────────────────────────────────────────

    public List<RemoteBranch> GetRemoteBranches()
    {
        var output = Git("for-each-ref", "--sort=-committerdate",
            "--format=%(refname:short)|%(committerdate:short)|%(authorname)|%(subject)",
            "refs/remotes/origin/");

        var result = new List<RemoteBranch>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 4);
            if (parts.Length < 4) continue;
            result.Add(new RemoteBranch
            {
                Name = parts[0].Replace("origin/", ""),
                Date = parts[1],
                Author = parts[2],
                LastCommit = parts[3]
            });
        }
        return result;
    }

    // ── Metadados de branches (para filtros) ───────────────────

    public List<BranchMetadata> GetBranchesMetadata()
    {
        var output = Git("for-each-ref", "--sort=-committerdate",
            "--format=%(refname:short)|%(committerdate:short)|%(committerdate:iso8601)|%(authorname)",
            "refs/remotes/origin/", "refs/heads/");

        var result = new List<BranchMetadata>();
        var seen = new HashSet<string>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 4);
            if (parts.Length < 4) continue;
            var name = parts[0].Trim();
            var shortName = name.StartsWith("origin/") ? name["origin/".Length..] : name;
            if (!seen.Add(shortName)) continue;

            // Extrair prefixo/tipo do branch
            var prefix = "";
            var slashIdx = shortName.IndexOf('/');
            if (slashIdx > 0) prefix = shortName[..slashIdx];

            result.Add(new BranchMetadata
            {
                FullName = name,
                ShortName = shortName,
                DateShort = parts[1].Trim(),
                Date = DateTime.TryParse(parts[2].Trim(), out var d) ? d : DateTime.MinValue,
                Author = parts[3].Trim(),
                Prefix = prefix
            });
        }
        return result;
    }

    // ── Helpers ────────────────────────────────────────────────────

    private List<CommitInfo> ParseCommits(string log)
    {
        var result = new List<CommitInfo>();
        foreach (var line in log.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 5);
            if (parts.Length < 5) continue;
            result.Add(new CommitInfo
            {
                Hash = parts[0],
                Author = parts[1],
                RelativeDate = parts[2],
                Date = DateTime.TryParse(parts[3], out var d) ? d : DateTime.MinValue,
                Message = parts[4]
            });
        }
        return result;
    }

    // ── Informacoes do branch atual ──────────────────────────────

    /// <summary>Retorna commits recentes do branch atual</summary>
    public List<CommitInfo> GetRecentCommits(int count = 30)
    {
        var log = Git("log", $"-{count}", "--format=%h|%an|%ar|%ai|%s");
        return ParseCommits(log);
    }

    /// <summary>Retorna arquivos modificados localmente (nao commitados)</summary>
    public List<FileChange> GetLocalChanges()
    {
        var status = Git("status", "--porcelain");
        var result = new List<FileChange>();
        foreach (var line in status.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length < 3) continue;
            var code = line[..2].Trim();
            var path = line[3..].Trim();
            result.Add(new FileChange
            {
                Status = code switch
                {
                    "M" => "Modificado",
                    "A" => "Adicionado",
                    "D" => "Removido",
                    "R" => "Renomeado",
                    "??" => "Nao rastreado",
                    "MM" => "Modificado (staged+unstaged)",
                    "AM" => "Adicionado (modificado)",
                    _ => code
                },
                StatusCode = code.Length > 0 ? code[0] : ' ',
                FilePath = path
            });
        }
        return result;
    }

    /// <summary>Retorna info de ahead/behind do branch atual vs remote</summary>
    public (int ahead, int behind) GetAheadBehind()
    {
        var branch = GetCurrentBranch();
        var tracking = Git("rev-parse", "--abbrev-ref", $"{branch}@{{upstream}}");
        if (string.IsNullOrEmpty(tracking)) return (0, 0);

        var revList = Git("rev-list", "--left-right", "--count", $"{tracking}...HEAD");
        if (string.IsNullOrEmpty(revList)) return (0, 0);

        var parts = revList.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return (0, 0);

        int.TryParse(parts[0].Trim(), out var behind);
        int.TryParse(parts[1].Trim(), out var ahead);
        return (ahead, behind);
    }

    /// <summary>Retorna branches locais com data do ultimo commit</summary>
    public List<RemoteBranch> GetLocalBranchesInfo()
    {
        var output = Git("for-each-ref", "--sort=-committerdate",
            "--format=%(refname:short)|%(committerdate:short)|%(authorname)|%(subject)",
            "refs/heads/");
        var result = new List<RemoteBranch>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 4);
            if (parts.Length < 4) continue;
            result.Add(new RemoteBranch
            {
                Name = parts[0].Trim(),
                Date = parts[1].Trim(),
                Author = parts[2].Trim(),
                LastCommit = parts[3].Trim()
            });
        }
        return result;
    }

    /// <summary>Retorna stashes existentes</summary>
    public List<string> GetStashes()
    {
        var output = Git("stash", "list");
        if (string.IsNullOrEmpty(output)) return new List<string>();
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    // ── Clone de repositório remoto ─────────────────────────────

    /// <summary>Diretório cache para repositórios clonados via URL</summary>
    public static string GetCacheDir(string? customPath = null)
    {
        var dir = !string.IsNullOrWhiteSpace(customPath)
            ? customPath
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repos_cache");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>Extrai nome do repositório a partir da URL</summary>
    public static string GetRepoNameFromUrl(string url)
    {
        url = url.TrimEnd('/');
        if (url.EndsWith(".git")) url = url[..^4];
        var lastSlash = url.LastIndexOf('/');
        return lastSlash >= 0 ? url[(lastSlash + 1)..] : url;
    }

    /// <summary>Retorna o caminho local para um repositório clonado via URL</summary>
    public static string GetCachedRepoPath(string url, string? customCachePath = null)
    {
        var repoName = GetRepoNameFromUrl(url);
        return Path.Combine(GetCacheDir(customCachePath), repoName);
    }

    /// <summary>Verifica se um repositório já foi clonado no cache</summary>
    public static bool IsCachedRepo(string url, string? customCachePath = null)
    {
        var path = GetCachedRepoPath(url, customCachePath);
        return Directory.Exists(Path.Combine(path, ".git"));
    }

    /// <summary>
    /// Clona um repositório remoto para o cache local.
    /// Reporta progresso via callback. Retorna o caminho local.
    /// </summary>
    public static (string path, string error) CloneRepository(string url, Action<string>? onProgress = null, string? customCachePath = null)
    {
        var repoName = GetRepoNameFromUrl(url);
        var targetPath = GetCachedRepoPath(url, customCachePath);

        // Se já existe, faz fetch ao invés de clonar novamente
        if (Directory.Exists(Path.Combine(targetPath, ".git")))
        {
            onProgress?.Invoke("Repositório já existe no cache. Atualizando...");
            var fetchResult = RunGitStatic(targetPath, "fetch", "--prune", "--no-tags");
            if (fetchResult.exitCode != 0)
                return (targetPath, $"Erro ao atualizar: {fetchResult.stderr}");
            onProgress?.Invoke("Repositório atualizado com sucesso.");
            return (targetPath, "");
        }

        onProgress?.Invoke($"Clonando {repoName}...");

        var psi = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        psi.ArgumentList.Add("clone");
        psi.ArgumentList.Add("--progress");
        psi.ArgumentList.Add(url);
        psi.ArgumentList.Add(targetPath);

        using var proc = Process.Start(psi)!;

        // Git envia progresso via stderr
        var errorBuilder = new StringBuilder();
        proc.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            var line = e.Data.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                onProgress?.Invoke(line);
                errorBuilder.AppendLine(line);
            }
        };
        proc.BeginErrorReadLine();
        proc.StandardOutput.ReadToEnd();
        proc.WaitForExit(600_000); // 10 min timeout

        if (proc.ExitCode != 0)
        {
            // Limpar diretório parcial
            try { if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true); } catch { }
            return ("", $"Erro ao clonar repositório (exit code {proc.ExitCode}):\n{errorBuilder}");
        }

        onProgress?.Invoke("Clone concluído com sucesso!");
        return (targetPath, "");
    }

    /// <summary>Executa git em um diretório específico (estático)</summary>
    private static (string stdout, string stderr, int exitCode) RunGitStatic(string workDir, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit(60_000);
        return (stdout.Trim(), stderr.Trim(), proc.ExitCode);
    }

    /// <summary>Valida se uma URL de git é acessível (ls-remote)</summary>
    public static (bool ok, string error) ValidateGitUrl(string url)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.ArgumentList.Add("ls-remote");
        psi.ArgumentList.Add("--heads");
        psi.ArgumentList.Add(url);

        try
        {
            using var proc = Process.Start(psi)!;
            proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit(30_000);
            return proc.ExitCode == 0 ? (true, "") : (false, stderr.Trim());
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
