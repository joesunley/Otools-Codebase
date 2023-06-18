using ownsmtp.logging;
using System.Runtime.InteropServices;

namespace OTools.AvaCommon;

public class InputMonitor
{
    private PaintBox _paintBox;

    public InputMonitor(PaintBox paintBox)
    {
        _paintBox = paintBox;
        Events();
    }   


    void Events()
    {
        _paintBox.PointerPressed += (s, args) => ODebugger.Info($"Pointer Pressed");
        _paintBox.PointerReleased += (s, args) => ODebugger.Info($"Pointer Released: {args.InitialPressMouseButton}");
        _paintBox.PointerWheelChanged += (s, args) => ODebugger.Info("Pointer Wheel Changed");
        _paintBox.KeyDown += (s, args) => ODebugger.Info($"Key Down: {args.Key}");
        _paintBox.KeyUp += (s, args) => ODebugger.Info($"Key Up: {args.Key}");
    }

}

public static class DebugTools
{
    [DllImport("Kernel32")]
    public static extern void AllocConsole();

    [DllImport("Kernel32")]
    public static extern void FreeConsole();
}