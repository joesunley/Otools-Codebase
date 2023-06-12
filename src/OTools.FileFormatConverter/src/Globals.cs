global using static Globals;
global using  Sunley.Mathematics;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Console")]

public static class Globals
{
    public static void Log(object message, int logCode = 0)
    {
        Debug.WriteLine(message);
    }

    public static void Assert(bool condition, string message = "")
    {
        Debug.Assert(condition, message);
    }
}