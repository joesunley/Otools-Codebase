using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Diagnostics.Tracing;

namespace OTools.MapMaker;

public static class Manager
{
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

    public static event Action<Symbol>? ActiveSymbolChanged;
    public static event Action<Tool>? ActiveToolChanged;

    public static MapMakerSettings Settings { get; set; } = MapMakerSettings.Default;
}

public enum Tool { Edit, Point, Path, Rectangle, Fill, Text, Ellipse, Freehand }