using Avalonia.Input;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Collections.Generic;

namespace OTools.MapMaker;

public class MapDraw
{
    private (PointDraw point, SimplePathDraw sPath) draws;

    private Active active;

    public MapDraw()
    {
        Manager.ActiveSymbolChanged += SymbolChanged;

        //ViewManager.MouseDown += args => MouseDown(args.)

        ViewManager.MouseUp += args => MouseUp(args.InitialPressMouseButton);
        ViewManager.MouseMove += _ => MouseMove();

        ViewManager.KeyUp += args => KeyUp(args.Key);
    }
    
    public void SymbolChanged(Symbol sym)
    {
        switch (sym)
        {
            case PointSymbol point:
                active = Active.Point;
                draws.point = new(point);
                draws.point.Start();
                break;
            case IPathSymbol path:
                active = Active.SimplePath;
                draws.sPath = new(path);
                break;
        }
    }

    public void MouseDown(MouseButton mouse)
    {
        
    }

    public void MouseUp(MouseButton mouse)
    {
        switch (active)
        {
            case Active.Point: if (mouse == MouseButton.Left) draws.point.End(); break;
            case Active.SimplePath:
                if (mouse == MouseButton.Left)
                {
                    draws.sPath.Start();
                    draws.sPath.NewPoint();
                }
                else if (mouse == MouseButton.Right)
                {
                    draws.sPath.End();
                }

                break;
        }
    }

    public void MouseMove()
    {
        switch (active)
        {
            case Active.Point: draws.point.Update(); break;
            case Active.SimplePath: 
                draws.sPath.Update(); 
                draws.sPath.Idle(); 
                break;  
        }
    }

    public void KeyUp(Key key)
    {
        switch (active)
        {
            case Active.SimplePath:
                switch (key)
                {
                    case Key.Enter:
                        draws.sPath.Complete();
                        break;
                    case Key.Escape:
                        draws.sPath.Cancel();
                        break;
                }
                break;
        }
    }

    private enum Active { Point, SimplePath, ComplexPath }
}

public class PointDraw
{
    private PointInstance _inst;
    private IMapRender _render;

    private bool _active;

    public PointDraw(PointSymbol sym)
    {
        _inst = new(Manager.ActiveLayer, sym, vec2.Zero, 0f);

        if (Manager.MapRender is null)
            Manager.MapRender = new MapRender(Manager.ActiveMap!);
        _render = Manager.MapRender;

        _active = false;
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        _inst.Centre = ViewManager.MousePosition;
        _inst.Opacity = Manager.Settings.Draw_Opacity;

        var render = _render.RenderPointInstance(_inst);
        ViewManager.Add(_inst.Id, render);
    }

    public void Update()
    {
        if (!_active) return;

        _inst.Centre = ViewManager.MousePosition;

        //if (ViewManager.IsMouseOutsideBounds())
        //    _inst.Opacity = 0f;
        //else _inst.Opacity = 1f;

        var render = _render.RenderPointInstance(_inst);
        ViewManager.Update(_inst.Id, render);
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        _inst.Centre = ViewManager.MousePosition;
        _inst.Opacity = 1f;

        Manager.ActiveMap!.Instances.Add(_inst);

        var render = _render.RenderPointInstance(_inst);
        ViewManager.Update(_inst.Id, render);

        _inst = new(_inst.Layer, _inst.Symbol, _inst.Centre, _inst.Rotation);
        Start();
    }
}

public class SimplePathDraw
{
    private PathInstance _inst;
    private IMapRender _render;

    private bool _active;
    private List<vec2> _points;

    public SimplePathDraw(IPathSymbol sym)
    {
        _inst = sym switch
        {
            LineSymbol l => new LineInstance(Manager.ActiveLayer, l, new(), false, 0f),
            AreaSymbol a => new AreaInstance(Manager.ActiveLayer, a, new(), false, 0f),
            _ => throw new InvalidOperationException(),
        };

        if (Manager.MapRender is null)
            Manager.MapRender = new MapRender(Manager.ActiveMap!);
        _render = Manager.MapRender;

        _active = false;
        _points = new();
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        _inst = _inst.Clone();
        _points = new() { ViewManager.MousePosition, ViewManager.MousePosition };

        _inst.Segments.Reset(_points);
        _inst.Opacity = Manager.Settings.Draw_Opacity;
        _inst.IsClosed = false;

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Add(_inst.Id, render);
    }

    public void Update()
    {
        if (!_active) return;

        _points[^1] = ViewManager.MousePosition;

        _inst.Segments.Reset(_points);
        
        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);
    }

    public void NewPoint()
    {
        if (!_active) return;

        _points.Add(ViewManager.MousePosition);

        _inst.Segments.Reset(_points);

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        _points.Add(ViewManager.MousePosition);

        if (_points[0] == _points[1])
            _points.RemoveAt(1);

        _inst.Segments.Reset(_points);
        _inst.Opacity = 1f;

        Manager.ActiveMap!.Instances.Add(_inst);

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);

        
    }

    public void Complete()
    {
        if (!_active) return;
        _active = false;

        _points.Add(ViewManager.MousePosition);

        
        if (_points[0] == _points[1])
            _points.RemoveAt(1);

        _inst.Segments.Reset(_points);
        _inst.Opacity = 1f;
        _inst.IsClosed = true;

        Manager.ActiveMap!.Instances.Add(_inst);

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);
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

        ViewManager.Remove(_inst.Id);
    }
}

public class MapSelect
{

}

public class PointSelect
{

}

file static class Extension
{
    public static void Reset(this PathCollection pC, IEnumerable<vec2> points)
    {
        pC.Clear();
        pC.Add(new LinearPath(points));
    }
}