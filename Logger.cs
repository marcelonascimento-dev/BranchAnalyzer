namespace BranchAnalyzer;

/// <summary>
/// Simple file logger. Thread-safe, auto-rotates at 5 MB.
/// Writes to &lt;exe dir&gt;/logs/branchanalyzer.log
/// </summary>
public static class Logger
{
    private static readonly object _lock = new();
    private static readonly string LogDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "logs");
    private static readonly string LogFile = Path.Combine(LogDir, "branchanalyzer.log");
    private static readonly string LogFileOld = Path.Combine(LogDir, "branchanalyzer.log.old");
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);

    public static void Error(string message, Exception ex)
        => Write("ERROR", $"{message} | {ex.GetType().Name}: {ex.Message}");

    private static void Write(string level, string message)
    {
        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                RotateIfNeeded();

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var line = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFile, line);
            }
            catch
            {
                // Last resort: logging itself must never crash the app
            }
        }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            if (!File.Exists(LogFile)) return;
            var info = new FileInfo(LogFile);
            if (info.Length < MaxFileSize) return;

            if (File.Exists(LogFileOld))
                File.Delete(LogFileOld);
            File.Move(LogFile, LogFileOld);
        }
        catch
        {
            // Rotation failure is non-critical
        }
    }
}
