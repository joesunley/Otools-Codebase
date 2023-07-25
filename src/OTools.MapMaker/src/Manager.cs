using Avalonia.Controls;
using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;

namespace OTools.MapMaker;

public static class Manager
{
    static Manager()
    {
        _menuManager = new();
    }

    public static PaintBox? PaintBox { get; set; }
    public static Map? Map { get; set; }
    public static int Layer { get; set; } = 0;

    private static Symbol? _activeSymbol;
    public static Symbol? Symbol
    {
        get => _activeSymbol;

        set
        {
            _activeSymbol = value;
            ActiveSymbolChanged?.Invoke(_activeSymbol!);
        }
    }

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

    public static IMapRenderer2D? MapRenderer { get; set; }

    public static MapDraw? MapDraw { get; set; }
    public static MapEdit? MapEdit { get; set; }

    public static event Action<Symbol>? ActiveSymbolChanged;
    public static event Action<Tool>? ActiveToolChanged;

    public static MapMakerSettings Settings { get; set; } = MapMakerSettings.Default;

    private static MenuManager _menuManager;

    public static void MenuClickHandle(MenuItem item) => _menuManager.Call(item);

    public static void ReRender()
    {
        if (PaintBox is null || MapRenderer is null)
            return;

        var render = MapRenderer.RenderMap();
        PaintBox.Load(render.Select(x => (x.Item1.Id, x.Item2)));
    }
}

public enum Tool { Edit, Point, Path, Rectangle, Fill, Text, Ellipse, Freehand }