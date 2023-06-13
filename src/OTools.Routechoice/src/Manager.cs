using OTools.AvaCommon;

namespace OTools.Routechoice;

public static class Manager
{
    public static PaintBox? PaintBox { get; set; }
	public static Course? Course { get; set; }

    private static Tool _activeTool;

    public static Tool Tool
    {
        get => _activeTool;

        set
        {
            _activeTool = value;
            ActiveToolChanged?.Invoke(_activeTool);
        }
    }

    public static event Action<Tool>? ActiveToolChanged;
}

public enum Tool { Course, Routechoice }