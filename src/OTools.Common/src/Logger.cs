using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace OTools.Common;

public enum LogLevel
{
    Debug = 5,
    Info = 4,
    Warn = 3,
    Error = 2,
    Fatal = 1,
    None = 0,
}

public class Logger
{
    private List<Action<string, LogLevel>> _targets;
    private LogLevel _logLevel;

    public Logger()
    {
        _targets = new();
    }

    private void Log(string message, LogLevel level)
    {
        if (_logLevel < level)
            return;

        foreach (var target in _targets)
            target(message, level);
    }

    public void Debug(string message, params object[] args) => Log(string.Format(message, args), LogLevel.Debug);
    public void Info(string message, params object[] args) => Log(string.Format(message, args), LogLevel.Info);
    public void Warn(string message, params object[] args) => Log(string.Format(message, args), LogLevel.Warn);
    public void Error(string message, params object[] args) => Log(string.Format(message, args), LogLevel.Error);
    public void Fatal(string message, params object[] args) => Log(string.Format(message, args), LogLevel.Fatal);

    public void SetLogLevel(LogLevel level) => _logLevel = level;

    public void AddTarget(Action<string, LogLevel> target) => _targets.Add(target);
}

public static class StaticLogger
{
    private static Logger _logger;

    static StaticLogger()
    {
        _logger = new();
    }

    public static void LogDebug(string message) => _logger.Debug(message);
    public static void LogInfo(string message) => _logger.Info(message);
    public static void LogWarn(string message) => _logger.Warn(message);
    public static void LogError(string message) => _logger.Error(message);
    public static void LogFatal(string message) => _logger.Fatal(message);

    public static void AddConsoleTarget()
    {
        _logger.AddTarget((s, l) =>
        {
            Console.ForegroundColor = l switch
            {
                LogLevel.Fatal => ConsoleColor.Red,
                LogLevel.Error => ConsoleColor.Magenta,
                LogLevel.Warn => ConsoleColor.Green,
                LogLevel.Info => ConsoleColor.Cyan,
                _ => ConsoleColor.White,
            };

            string o = $"[{Thread.CurrentThread.ManagedThreadId:D4}] [{DateTime.Now.ToString("HH:mm:ss.ffff")}] ({l}): {s}";
            Console.WriteLine(o);
        });
    }

    public static void AddDebugTarget()
    {
        _logger.AddTarget((s, l) =>
        {
            string o = $"[{Thread.CurrentThread.ManagedThreadId:D4}] [{DateTime.Now.ToString("HH:mm:ss.ffff")}] ({l}): {s}";
            Console.WriteLine(o);
        });
    }
}