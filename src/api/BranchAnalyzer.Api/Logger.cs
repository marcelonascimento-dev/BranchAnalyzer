namespace BranchAnalyzer.Api;

public static class Logger
{
    private static readonly string LogDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "logs");
    private static readonly string LogFile = Path.Combine(LogDir, "branchanalyzer-api.log");
    private static readonly object Lock = new();
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public static void Info(string message) => Log("INFO", message);
    public static void Warn(string message) => Log("WARN", message);
    public static void Error(string message) => Log("ERROR", message);
    public static void Error(string message, Exception ex) => Log("ERROR", $"{message}: {ex.Message}");

    private static void Log(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            RotateIfNeeded();
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            lock (Lock)
            {
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
        }
        catch { }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            if (File.Exists(LogFile) && new FileInfo(LogFile).Length > MaxFileSize)
            {
                var rotated = Path.Combine(LogDir, $"branchanalyzer-api.{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.Move(LogFile, rotated);
            }
        }
        catch { }
    }
}
