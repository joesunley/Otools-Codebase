/*
 * CreatedBy: JosephPhilbert}
 * User: jphilbert
 * Date: 3/27/2015
 * Time: 8:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace ownsmtp.logging;

static class Logger
{

}

public interface ServerLogger
{
    ServerLogLevel LogLevel { get; set; }

    void Debug(string text, params object[] args);
    void Info(string text, params object[] args);
    void Warn(string text, params object[] args);
    void Error(string text, params object[] args);
    void Error(Exception ex);
    void Error(Exception ex, string text, params object[] args);
}

public enum ServerLogLevel
{
    Debug = 4,
    Info = 3,
    Warn = 2,
    Error = 1,
    None = 0
}
public class ConsoleLogger : ServerLogger
{

    public ServerLogLevel LogLevel { get; set; }

    public ConsoleLogger(ServerLogLevel logLevel)
    {
        LogLevel = logLevel;
    }

    public void Info(string text, params object[] args)
    {
        if (LogLevel < ServerLogLevel.Info)
            return;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Conlog("(I) ", text, args);
    }

    public void Warn(string text, params object[] args)
    {

        if (LogLevel < ServerLogLevel.Warn)
            return;
        Console.ForegroundColor = ConsoleColor.Green;
        Conlog("(W) ", text, args);
    }

    public void Error(Exception ex)
    {

        if (LogLevel < ServerLogLevel.Error)
            return;
        Console.ForegroundColor = ConsoleColor.Red;
        Conlog("(E) ", ex.ToString());
    }

    public void Error(string text, params object[] args)
    {

        if (LogLevel < ServerLogLevel.Error)
            return;
        Console.ForegroundColor = ConsoleColor.Red;
        Conlog("(E) ", text, args);
    }

    public void Error(Exception ex, string text, params object[] args)
    {

        if (LogLevel < ServerLogLevel.Error)
            return;
        Console.ForegroundColor = ConsoleColor.Red;
        Conlog("(E) ", text, args);
    }

    public void Debug(string text, params object[] args)
    {

        if (LogLevel < ServerLogLevel.Debug)
            return;
        Conlog("(D) ", text, args);
    }

    private static void Conlog(string prefix, string text, params object[] args)
    {
        //            If you want to add unique thread identifier
        
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.Write("[{0:D4}] [{1}] ", threadId, DateTime.Now.ToString("HH:mm:ss.ffff"));
        //Console.Write(DateTime.Now.ToString("HH:mm:ss.ffff"));
        
        Console.Write(prefix);
        Console.WriteLine(text, args);
        Console.ResetColor();
    }
}

public static class ODebugger
{
    private static ServerLogger _logger;

    static ODebugger()
    {
        _logger = new ConsoleLogger(ServerLogLevel.Info);
    }

    public static void SetLevel(ServerLogLevel level)
    {
        _logger.LogLevel = level;
    }

    public static void Debug(string text, params object[] args)
    {
        _logger.Debug(text, args);
    }

    public static void Info(string text, params object[] args)
    {
        _logger.Info(text, args);
    }

    public static void Warn(string text, params object[] args)
    {
        _logger.Warn(text, args);
    }

    public static void Error(string text, params object[] args)
    {
        _logger.Error(text, args);
    }

    public static void Error(Exception ex)
    {
        _logger.Error(ex);
    }

    public static void Error(Exception ex, string text, params object[] args)
    {
        _logger.Error(ex, text, args);
    }

    public static void Conlog(string prefix, string text, params object[] args)
    {
        Console.WriteLine(prefix + text, args);
    }


    public static void Assert(bool condition, string? message = null) 
        => System.Diagnostics.Debug.Assert(condition, message);
}