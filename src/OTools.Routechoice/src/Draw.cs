using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Maps;
using ownsmtp.logging;
using r = OTools.ObjectRenderer2D;

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
		_draws.course = new(_paintBox);
	}

	public void MouseUp(MouseButton mouse)
	{
		switch (Manager.Tool)
		{
			case Tool.Course:
				if (mouse == MouseButton.Left)
				{
					_draws.course.Start();
					_draws.course.NewPoint();
				}
				else if (mouse == MouseButton.Right)
				{
                    _draws.course.End();
                }
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
				_draws.course.Update();
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
	private Guid _staticId, _changingId;

	private List<vec2> points;

	private bool _active;

	private PaintBox paintBox;

	private float _radius = 10f, _thickness = 2f;

	private r.IMapRenderer2D _renderer;

	public CourseDraw(PaintBox paintBox)
	{
		points = new();

		_active = false;
		this.paintBox = paintBox;

		_renderer = new r.MapRenderer2D(Manager.Map);
	}

	public void Start()
	{
		  if (_active) return;
		_active = true;

		points.Clear();

		points.Add(paintBox.MousePosition);
		points.Add(paintBox.MousePosition);

		_staticId = Guid.NewGuid();
		_changingId = Guid.NewGuid();

		var changing = CreateChanging();

		paintBox.Add(_changingId, changing);
	}

	public void Update()
	{
		if (!_active) return;

		points[^1] = paintBox.MousePosition;

		var changing = CreateChanging();

		paintBox.Update(_changingId, changing);
	}

	public void NewPoint()
	{
		if (!_active) return;

		points.Add(paintBox.MousePosition);

		var changing = CreateChanging();

		paintBox.Update(_changingId, changing);
	}

	public void End()
	{
		if (!_active) return;
		_active = false;

		points.Add(paintBox.MousePosition);

		points = points.Distinct().ToList();

		// Add to manager

		Manager.Course = new() { Controls = new(points), };

		var changing = CreateChanging();
		paintBox.Update(_changingId, changing);

		points.Clear();

		RcGame g = new();

		g.Start();
	}

	private IEnumerable<Shape> CreateChanging()
	{
//		vec2 start = points[0], finish = points[^1];

//		var controls = points.Distinct().Skip(1).SkipLast(1).ToList();
		
//		ODebugger.Warn($"{controls.Count}");

		LineSymbol symLine = (LineSymbol)Manager.Map.Symbols["Line"];
//		PointSymbol symStart = (PointSymbol)Manager.Map.Symbols["Start"];
//		PointSymbol symControl = (PointSymbol)Manager.Map.Symbols["Control"];
//		PointSymbol symFinish = (PointSymbol)Manager.Map.Symbols["Finish"];

		LineInstance l = new(0, symLine, new(points), false);
//		PointInstance sk = new(0, symStart, start, 0f); // Change Angle
//		var cs = controls.Select(p => new PointInstance(0, symControl, p, 0f));
//		PointInstance f = new(0, symFinish, finish, 0f); 

		var render = _renderer.RenderPathInstance(l);
//			.Concat(_renderer.RenderPointInstance(s))
//			.Concat(_renderer.RenderInstances(cs))
//			.Concat(_renderer.RenderPointInstance(f));

		//var es = controls.Select(p => new Ellipse { Width = 10, Height = 10, Fill = Brushes.Red, Tag = p});
		//es.ToList().ForEach(x => x.SetTopLeft((vec2)x.Tag!));


		return ObjConvert.ConvertCollection(render);
	}

	private IEnumerable<Shape> CreateStatic()
    {
		if (points.Count <= 1)
			return Enumerable.Empty<Shape>();

        vec2 start = points[0];
        PointSymbol symStart = (PointSymbol)Manager.Map.Symbols["Start"];
        PointInstance s = new(0, symStart, start, 0f); // Change Angle

        IEnumerable<r.IShape> render = _renderer.RenderInstance(s);

        var controls = points.Skip(1).SkipLast(1).ToList();

        if (!controls.Any())
            return ObjConvert.ConvertCollection(render);

		LineSymbol symLine = (LineSymbol)Manager.Map.Symbols["Line"];
		LineInstance l = new(0, symLine, new(points), false);

		render = render.Concat(_renderer.RenderInstance(l));

		return ObjConvert.ConvertCollection(render);
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