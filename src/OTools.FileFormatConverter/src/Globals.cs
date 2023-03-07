global using static Globals;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Console")]

public static class Globals
{
    public static void Log(object message, int logCode = 0)
    {
        int outCode = 0;

        if (outCode == 0)
            Debug.WriteLine(message);

        else if (logCode == outCode)
            Debug.WriteLine(message);
    }

    public static void Assert(bool condition, string message = "")
    {
        Debug.Assert(condition, message);
    }
}