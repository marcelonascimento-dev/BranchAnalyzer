using System.Text.Json;

namespace BranchAnalyzer.Api;

public class AppSettings
{
    private static readonly string SettingsFile = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // ── Repositório ──────────────────────────────────────────────────
    public string LastRepoPath { get; set; } = "";
    public string? LastRepoUrl { get; set; }
    public List<string> RecentRepoPaths { get; set; } = new();

    // ── Branches usados por último ───────────────────────────────────
    public string LastBranchA { get; set; } = "";
    public string LastBranchB { get; set; } = "";
    public string LastBatchReceptor { get; set; } = "";

    // ── Janela ───────────────────────────────────────────────────────
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public int WindowX { get; set; }
    public int WindowY { get; set; }
    public bool WindowMaximized { get; set; }
    public int LastSelectedTab { get; set; }

    // ── Comportamento ────────────────────────────────────────────────
    public bool FetchOnOpen { get; set; } = false;

    // ── Cache de repositórios clonados ────────────────────────────
    public string CloneCachePath { get; set; } = "";

    // ── Métodos ──────────────────────────────────────────────────────

    public void AddRecentRepo(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        RecentRepoPaths.Remove(path);
        RecentRepoPaths.Insert(0, path);

        if (RecentRepoPaths.Count > 10)
            RecentRepoPaths.RemoveRange(10, RecentRepoPaths.Count - 10);

        LastRepoPath = path;
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsFile, json);
        }
        catch
        {
            // Falha silenciosa — settings não são críticas
        }
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new();
            }
        }
        catch
        {
            // Arquivo corrompido — retorna default
        }
        return new();
    }
}
