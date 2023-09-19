using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;

namespace OTools.MapMaker;

public sealed class MapMakerInstance
{
    public PaintBox PaintBox { get; set; }
    public Map Map { get; set; }
    public int Layer { get; set; } = 0;

    private Symbol? _activeSymbol;
    public Symbol? ActiveSymbol
    {
        get => _activeSymbol;

        set
        {
            _activeSymbol = value;
            ActiveSymbolChanged?.Invoke(_activeSymbol!);

            ActiveTool = _activeSymbol switch
            {
                PointSymbol => Tool.Point,
                IPathSymbol => Tool.Path,
                _ => Tool.Edit,
            };
        }
    }
    public event Action<Symbol>? ActiveSymbolChanged;

    private Tool _activeTool;
    public Tool ActiveTool
    {
        get => _activeTool;

        set
        {
            _activeTool = value;
            ActiveToolChanged?.Invoke(_activeTool);
        }
    }
    public event Action<Tool>? ActiveToolChanged;

    public MapMakerSettings Settings { get; set; }
    
    public MenuManager MenuManager { get; set; }

    public IMapRenderer2D MapRenderer { get; set; }

    public MapDraw MapDraw { get; set; }
    public MapEdit MapEdit { get; set; }

    public MapMakerInstance(PaintBox paintBox, Map map, IMapRenderer2D? renderer, MapDraw? mapDraw, MapEdit? mapEdit, MapMakerSettings? settings)
    {
        PaintBox = paintBox;
        Map = map;

        MapRenderer = renderer ?? new MapRenderer2D(map);

        MapDraw = mapDraw ?? new(this);
        MapEdit = mapEdit ?? new(this);

        MenuManager = new(this);
        Settings = settings ?? MapMakerSettings.Default;
    }

    
    public void ReRender()
    {
        var render = MapRenderer.RenderMap();
        PaintBox.Load(render.Select(x => (x.Item1.Id, x.Item2)), Map.Id);
    }
}

public enum Tool { Edit, Point, Path, Rectangle, Fill, Text, Ellipse, Freehand }