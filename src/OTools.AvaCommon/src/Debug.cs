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


	private void Events()
    {
        _paintBox.PointerPressed += (_, _) => LogInfo($"Pointer Pressed");
        _paintBox.PointerReleased += (_, args) => LogInfo($"Pointer Released: {args.InitialPressMouseButton}");
        _paintBox.PointerWheelChanged += (_, _) => LogInfo("Pointer Wheel Changed");
        _paintBox.KeyDown += (_, args) => LogInfo($"Key Down: {args.Key}");
        _paintBox.KeyUp += (_, args) => LogInfo($"Key Up: {args.Key}");
    }
}

public static class DebugTools
{
    [DllImport("Kernel32")]
    public static extern void AllocConsole();

    [DllImport("Kernel32")]
    public static extern void FreeConsole();
}