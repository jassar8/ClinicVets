using System.Text;

namespace ClinicVets.Desktop.Stability;

/// <summary>Temporary stability logging until all navigation paths are verified.</summary>
public static class AppStability
{
    private static readonly object Gate = new();
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClinicVets",
        "stability.log");

    public static void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                LogException("AppDomain.UnhandledException", ex, fatal: true);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogException("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved();
        };
    }

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        lock (Gate)
        {
            File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    public static void LogException(string context, Exception ex, bool fatal = false)
    {
        Log($"{(fatal ? "FATAL" : "ERROR")} [{context}] {ex.GetType().Name}: {ex.Message}");
        Log(ex.StackTrace ?? "(no stack)");
        if (ex.InnerException is not null)
            Log($"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
    }

    public static string LogFilePath => LogPath;
}
