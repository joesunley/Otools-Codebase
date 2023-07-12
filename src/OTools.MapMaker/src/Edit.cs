using Avalonia.Input;
using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;

namespace OTools.MapMaker;

public class MapEdit
{
    private (PointEdit point, PathEdit path) draws;

    private bool isActive;
    private Active active;

    public MapEdit()
    {
        Manager.ActiveToolChanged += args => isActive = (args == Tool.Edit);
    }

    public void MouseDown(MouseButton mouse)
    {

    }

    public void MouseUp(MouseButton mouse)
    {

    }

    public void MouseMove()
    {

    }

    public void KeyUp(Key key)
    {
        switch (active)
        {
            case Active.Point:
                switch (key)
                {
                    case Key.Delete:
                        draws.point.Delete();
                        break;
                    case Key.Escape:
                        draws.point.Deselect();
                        break;
                }
                break;
        }
    }

    private enum Active { Point, Path }
}

public class PointEdit
{
    private PointInstance _instance;
    private PaintBox paintBox;
    private IMapRenderer2D _renderer;

    private Guid _helperId;

    private bool _active;

    public PointEdit(PaintBox paintBox, PointInstance instance)
    {
        _instance = instance;
        this.paintBox = paintBox;

        _active = false;    
        _helperId = Guid.NewGuid();

        if (Manager.MapRenderer is null)
            Manager.MapRenderer = new MapRenderer2D(Manager.Map!);
        _renderer = Manager.MapRenderer;
    }

    public void Select()
    {
        if (_active) return;
        _active = true;

        DrawHelpers();
    }

    public void Deselect()
    {
        if (!_active) return;
        _active = false;

        paintBox.Remove(_helperId);
    }   

    public void Move()
    {
        _instance.Centre = paintBox.MousePosition;

        paintBox.Update(_instance.Id, _renderer.RenderInstance(_instance).ConvertCollection());
    }

    public void Delete()
    {
        paintBox.Remove(_instance.Id);
        paintBox.Remove(_helperId);

        Manager.Map!.Instances.Remove(_instance);
    }

    void DrawHelpers()
    {
        IShape[] objs = { DrawBBox(), DrawHandle() };

        paintBox.AddOrUpdate(_helperId, objs.ConvertCollection());
    }

    private Rectangle DrawBBox()
    {
        vec4 bBox = BoundingBox.OfInstance(_instance);
        float offset = Manager.Settings.Select_BBoxOffset;

        return new()
        {
            TopLeft = bBox.XY - offset,
            Size = (bBox.ZW - bBox.XY + (offset * 2)).Abs(),

            BorderColour = Manager.Settings.Select_BBoxColour,
            BorderWidth = Manager.Settings.Select_BBoxLineWidth,
            DashArray = Manager.Settings.Select_BBoxDashArray,

            ZIndex = Manager.Settings.Select_BBoxZIndex,
        };

    }

    private Circle DrawHandle()
    {
        float radius = Manager.Settings.Select_HandleRadius;

        return new()
        {
            TopLeft = _instance.Centre - radius,
            Diameter = 2 * radius,

            BorderColour = Manager.Settings.Select_HandleAnchorColour,
            BorderWidth = Manager.Settings.Select_HandleLineWidth,

            ZIndex = Manager.Settings.Select_HandleZIndex,
        };
    }
}

public class PathEdit
{

    public void Select()
    {

    }

    public void Deselect()
    {

    }

    public void MovePoint()
    {

    }

    public void InsertPoint()
    {

    }

    public void RemovePoint()
    {

    }   



    public void Delete()
    {

    }
}