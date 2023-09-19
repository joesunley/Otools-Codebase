using Avalonia.Input;
using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System.Security.Permissions;

namespace OTools.MapMaker;

public class MapDraw
{
    private MapMakerInstance _instance;

    public MapDraw(MapMakerInstance instance)
    {
        _instance = instance;

        _instance.ActiveSymbolChanged += SymbolChanged;
        _instance.ActiveToolChanged += args => IsActive = (args != Tool.Edit);

        _instance.PaintBox.PointerMoved += (_, _) => MouseMove();
        _instance.PaintBox.PointerReleased += (_, args) => MouseUp(args.InitialPressMouseButton);
        _instance.PaintBox.KeyUp += (_, args) => KeyUp(args.Key);
    }

    private PointDraw? _pointDraw;
    private SimplePathDraw? _simplePathDraw;

    public bool IsActive { get; set; }

    private void SymbolChanged(Symbol symbol)
    {
        if (!IsActive) return;

        switch (symbol)
        {
            case PointSymbol point:
                _pointDraw = new(_instance, point);
                _simplePathDraw = null;
                _pointDraw.Start();
                break;
            case IPathSymbol path:
                _pointDraw = null;
                _simplePathDraw = new(_instance, path);
                break;
        }
    }
    private void MouseUp(MouseButton mouse)
    {
        if (!IsActive) return;

        switch (mouse)
        {
            case MouseButton.Left:
                _pointDraw?.End();

                _simplePathDraw?.Start();
                _simplePathDraw?.NewPoint();

                break;
            case MouseButton.Right:
                _simplePathDraw?.End();
                break;
        }
    }
    private void MouseMove()
    {
        if (!IsActive) return;

        _pointDraw?.Update();

        _simplePathDraw?.Update();
        _simplePathDraw?.Idle();
    }
    private void KeyUp(Key key)
    {
        switch (key)
        {
            case Key.Enter:
                _simplePathDraw?.Complete();
                break;
            case Key.Escape:
                _simplePathDraw?.Cancel();
                break;
        }
    }

    public void Start() => IsActive = true;
    public void Stop() => IsActive = false;
}

public class PointDraw
{
    private MapMakerInstance _mInstance;
    private PointInstance _inst;

    private bool _active;

    public PointDraw(MapMakerInstance mInstance, PointSymbol pSym)
    {
        _inst = new(mInstance.Layer, pSym, vec2.Zero, 0f);

        _mInstance = mInstance;

        _active = false;
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        _inst.Centre = _mInstance.PaintBox.MousePosition;
        _inst.Opacity = _mInstance.Settings.Draw_Opacity;

        var render = _mInstance.MapRenderer.RenderPointInstance(_inst).ConvertCollection();
        _mInstance.PaintBox.Add(_inst.Id, render);
    }

    public void Update()
    {
        if (!_active) return;

        _inst.Centre = _mInstance.PaintBox.MousePosition;

        var render = _mInstance.MapRenderer.RenderPointInstance(_inst).ConvertCollection();
        _mInstance.PaintBox.Add(_inst.Id, render);
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        _inst.Centre = _mInstance.PaintBox.MousePosition;
        _inst.Opacity = 1f;

        _mInstance.Map.Instances.Add(_inst);

        var render = _mInstance.MapRenderer.RenderPointInstance(_inst).ConvertCollection();
        _mInstance.PaintBox.Add(_inst.Id, render);

        _inst = new(_inst.Layer, _inst.Symbol, _inst.Centre, _inst.Rotation);
        Start();
    }
}

public class SimplePathDraw
{
    private MapMakerInstance _mInstance;
    private PathInstance _inst;

    private bool _active;
    private List<vec2> _points;
    private bool _drawGuide;

    public SimplePathDraw(MapMakerInstance mInstance, IPathSymbol pSym)
    {
        _inst = pSym switch
        {
            LineSymbol l => new LineInstance(mInstance.Layer, l, new(), false),
            AreaSymbol a => new AreaInstance(mInstance.Layer, a, new(), false, 0f, null),
            _ => throw new InvalidOperationException()
        };

        _mInstance = mInstance;

        _active = false;
        _points = new();
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        _inst = _inst.Clone();
        _points = new() { _mInstance.PaintBox.MousePosition, _mInstance.PaintBox.MousePosition };

        _inst.Segments.Reset(_points);
        _inst.Opacity = _mInstance.Settings.Draw_Opacity;
        _inst.IsClosed = false;

        _drawGuide = _inst is AreaInstance area &&
            (area.Symbol.Width == 0 || area.Symbol.Colour == Colour.Transparent);

        var render = _mInstance.MapRenderer.RenderPathInstance(_inst)
            .Concat(!_drawGuide ? 
                Enumerable.Empty<IShape>() :
                new IShape[] { new Line { Colour = _mInstance.Settings.Draw_BorderColour, Points = _points, Width = 1, ZIndex = 999 } })
            .ConvertCollection();

        _mInstance.PaintBox.Add(_inst.Id, render);
    }

    public void Update()
    {
        if (!_active) return;

        _points[^1] = _mInstance.PaintBox.MousePosition;
        _inst.Segments.Reset(_points);

        var render = _mInstance.MapRenderer.RenderPathInstance(_inst)
            .Concat(!_drawGuide ?
                Enumerable.Empty<IShape>() :
                new IShape[] { new Line { Colour = _mInstance.Settings.Draw_BorderColour, Points = _points, Width = 1, ZIndex = 999 } })
            .ConvertCollection();

        _mInstance.PaintBox.Update(_inst.Id, render);
    }

    public void NewPoint()
    {
        if (!_active) return;

        _points.Add(_mInstance.PaintBox.MousePosition);
        _inst.Segments.Reset(_points);

        var render = _mInstance.MapRenderer.RenderPathInstance(_inst).ConvertCollection();
        _mInstance.PaintBox.Update(_inst.Id, render);
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        _points.Add(_mInstance.PaintBox.MousePosition);

        if (vec2.Mag(_points[0], _points[^1]) < 1)
        {
            _active = true;
            Complete();
            return;
        }

        if (_points[0] == _points[1])
            _points.RemoveAt(1);

        _inst.Segments.Reset(_points);
        _inst.Opacity = 1f;

        _mInstance.Map.Instances.Add(_inst);

        var render = _mInstance.MapRenderer.RenderPathInstance(_inst).ConvertCollection();
        _mInstance.PaintBox.Update(_inst.Id, render);
    }

    public void Complete()
    {
        if (!_active) return;
        _active = false;

        _points.Remove(_points[^1]);

        if (_points[0] == _points[1])
            _points.RemoveAt(1);

        _inst.Segments.Reset(_points);
        _inst.Opacity = 1f;
        _inst.IsClosed = true;

        _mInstance.Map.Instances.Add(_inst);

        var render = _mInstance.MapRenderer.RenderPathInstance(_inst).ConvertCollection();
        _mInstance.PaintBox.Update(_inst.Id, render);
    }

    public void Idle()
    {

    }

    public void Cancel()
    {
        if (!_active) return;
        _active = false;

        _points = new();

        _inst = _inst.Clone();
        _inst.Segments.Clear();

        _mInstance.PaintBox.Remove(_inst.Id);
    }
}

file static class Extension
{
    internal static void Reset(this PathCollection pc, IEnumerable<vec2> points)
    {
        pc.Clear();
        pc.Add(new LinearPath(points));
    }
}