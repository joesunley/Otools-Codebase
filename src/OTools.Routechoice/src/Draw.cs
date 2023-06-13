using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using OTools.AvaCommon;
using OTools.Common;

namespace OTools.Routechoice;

public class Draw
{
    private (CourseDraw course, RoutechoiceDraw rc) _draws;

    private readonly PaintBox _paintBox;

    public Draw()
    {
        Assert(Manager.PaintBox != null);
        _paintBox = Manager.PaintBox!;

        _paintBox.PointerReleased += (_, args) => MouseUp(args.InitialPressMouseButton);
        _paintBox.MouseMoved += _ => MouseMove();
        _paintBox.KeyUp += (_, args) => KeyUp(args.Key);

        _draws.rc = new(_paintBox, 0xFF0000FF);
    }

    public void MouseUp(MouseButton mouse)
    {
        switch (Manager.Tool)
        {
            case Tool.Course:
                break;
            case Tool.Routechoice:
                if (mouse == MouseButton.Left)
                {
                    _draws.rc.Start();
                    _draws.rc.NewPoint();
                }
                else if (mouse == MouseButton.Right)
                {
                    _draws.rc.End();
                }
                break;
        }
    }

    public void MouseMove()
    {
        switch (Manager.Tool)
        {
            case Tool.Course:
                break;
            case Tool.Routechoice:
                _draws.rc.Update();
                break;
        }
    }

    public void KeyUp(Key key)
    {
        switch (Manager.Tool)
        {
            case Tool.Course:
                break;
            case Tool.Routechoice:
                if (key == Key.Escape)
                    _draws.rc.Cancel();
                break;
        }
    }

	public static Draw Start() => new();
}

public class CourseDraw
{
    private Guid _id;

    private uint colour;
    private List<vec2> points;

    private bool _active;

    private PaintBox paintBox;

    private float _radius = 10f, _thickness = 2f;

    public CourseDraw(PaintBox paintBox, uint col = 0xffa626ff)
    {
        colour = col;
        points = new();

        _active = false;
        this.paintBox = paintBox;
    }

    public void Start()
    {
          if (_active) return;
        _active = true;

        points.Clear();

        points.Add(paintBox.MousePosition);
        points.Add(paintBox.MousePosition);

        _id = Guid.NewGuid();


    }
}

public class RoutechoiceDraw
{
    private Guid _lineId, _pointsId;

    private uint colour;
    private List<vec2> points;
    
    private bool _active;

    private PaintBox paintBox;

    private float _radius = 10f, _cThickness = 2f, _lThickness = 2f;

    public RoutechoiceDraw(PaintBox paintBox, uint col)
    {
        colour = col;
        points = new();

        _active = false;
        this.paintBox = paintBox;
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        points.Clear();

        points.Add(paintBox.MousePosition);
        points.Add(paintBox.MousePosition);

        _lineId = Guid.NewGuid();
        _pointsId = Guid.NewGuid();

        var line = CreateLine();
        paintBox.Add(_lineId, line.Yield());
    }

    public void Update()
    {
        if (!_active) return;

        points[^1] = paintBox.MousePosition;

        var line = CreateLine();
        paintBox.Update(_lineId, line.Yield());
        paintBox.Update(_pointsId, CreateCircles());

    }

    public void NewPoint()
    {
        if (!_active) return;

        points.Add(paintBox.MousePosition);

        var line = CreateLine();
        paintBox.Update(_lineId, line.Yield());
        paintBox.Update(_pointsId, CreateCircles());
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        points.Add(paintBox.MousePosition);

        if (points[0] == points[1])
            points.RemoveAt(1);

        // Add to manager

        var line = CreateLine();
        paintBox.Update(_lineId, line.Yield());
        paintBox.Remove(_pointsId);

        points.Clear();
    }

    public void Cancel()
    {
        if (!_active) return;
        _active = false;

        points = new();

        paintBox.Remove(_lineId);
    }

    public void Idle()
    {
        if (_active) return;


    }

    private Polyline CreateLine()
    {
        Polyline line = new()
        {
            Stroke = ObjConvert.ColourToBrush(colour),
            StrokeThickness = _lThickness,
            Points = points.Select(p => new Point(p.X, p.Y)).ToList(),
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
        };

        return line;
    }
    private IEnumerable<Ellipse> CreateCircles()
    {
        return Enumerable.Empty<Ellipse>();

        foreach (vec2 p in points)
        {
            Ellipse e = new()
            {
                Stroke = ObjConvert.ColourToBrush(colour),
                StrokeThickness = _cThickness,
                //Fill = ObjConvert.ColourToBrush(0xffffffff),
                Width = _radius,
                Height = _radius,
            };

            e.SetTopLeft(p - (new vec2(_radius, _radius) / 2));

            //yield return e;
        }
    }
}