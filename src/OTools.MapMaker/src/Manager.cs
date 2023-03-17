using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Diagnostics.Tracing;

namespace OTools.MapMaker;

public static class Manager
{
    public static Map? ActiveMap { get; set; }
    public static int ActiveLayer { get; set; } = 0;

    private static Symbol? _activeSymbol;
    public static Symbol? ActiveSymbol
    {
        get => _activeSymbol;

        set
        {
            _activeSymbol = value;
            ActiveSymbolChanged?.Invoke(_activeSymbol!);
        }
    }

    public static IMapRender? MapRender { get; set; }

    public static event Action<Symbol>? ActiveSymbolChanged;

    public static MapMakerSettings Settings { get; set; } = MapMakerSettings.Default;
}