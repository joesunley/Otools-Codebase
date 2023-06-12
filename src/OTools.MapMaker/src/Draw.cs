using Avalonia.Input;
using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OTools.MapMaker;

public class MapDraw
{
	private (PointDraw point, SimplePathDraw sPath) draws;

	private bool isActive;
	private Active active;

	private readonly PaintBox paintBox;

	public MapDraw()
	{
		Manager.ActiveSymbolChanged += SymbolChanged;
		Manager.ActiveToolChanged += args => isActive = (args != Tool.Edit);

		Assert(Manager.PaintBox != null);
		paintBox = Manager.PaintBox!;

		//ViewManager.MouseDown += args => MouseDown(args.)

		paintBox.PointerReleased += (_, args) => MouseUp(args.InitialPressMouseButton);
		paintBox.MouseMoved += _ => MouseMove();
		paintBox.KeyUp += (_, args) => KeyUp(args.Key);
	}
	
	public void SymbolChanged(Symbol sym)
	{

		switch (sym)
		{
			case PointSymbol point:
				active = Active.Point;
				draws.point = new(point, paintBox);
				draws.point.Start();
				break;
			case IPathSymbol path:
				active = Active.SimplePath;
				draws.sPath = new(path, paintBox);
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

	private enum Active { None, Point, SimplePath, ComplexPath }
}

public class PointDraw
{
	private PointInstance _inst;
	private IMapRenderer2D _renderer;

	private bool _active;

	private readonly PaintBox paintBox;

	public PointDraw(PointSymbol sym, PaintBox paintBox)
	{
		_inst = new(Manager.Layer, sym, vec2.Zero, 0f);

		if (Manager.MapRenderer is null)
			Manager.MapRenderer = new MapRenderer2D(Manager.Map!);
		_renderer = Manager.MapRenderer;

		_active = false;
		this.paintBox = paintBox;
	}

	public void Start()
	{
		if (_active) return;
		_active = true;

		_inst.Centre = paintBox.MousePosition;
		_inst.Opacity = Manager.Settings.Draw_Opacity;

		var render = _renderer.RenderPointInstance(_inst).ConvertCollection();
		paintBox.Add(_inst.Id, render);
	}

	public void Update()
	{
		if (!_active) return;

		_inst.Centre = paintBox.MousePosition;

		//if (ViewManager.IsMouseOutsideBounds())
		//    _inst.Opacity = 0f;
		//else _inst.Opacity = 1f;

		var render = _renderer.RenderPointInstance(_inst).ConvertCollection();
		paintBox.Update(_inst.Id, render);
	}

	public void End()
	{
		if (!_active) return;
		_active = false;

		_inst.Centre = paintBox.MousePosition;
		_inst.Opacity = 1f;

		Manager.Map!.Instances.Add(_inst);

		var render = _renderer.RenderPointInstance(_inst).ConvertCollection();
		paintBox.Update(_inst.Id, render);

		_inst = new(_inst.Layer, _inst.Symbol, _inst.Centre, _inst.Rotation);
		Start();
	}
}

public class SimplePathDraw
{
	private PathInstance _inst;
	private IMapRenderer2D _renderer;

	private bool _active;
	private List<vec2> _points;

	private bool _drawGuide;

	private readonly PaintBox paintBox;

	public SimplePathDraw(IPathSymbol sym, PaintBox paintBox)
	{
		_inst = sym switch
		{
			LineSymbol l => new LineInstance(Manager.Layer, l, new(), false, 0f),
			AreaSymbol a => new AreaInstance(Manager.Layer, a, new(), false, 0f),
			_ => throw new InvalidOperationException(),
		};

		if (Manager.MapRenderer is null)
			Manager.MapRenderer = new MapRenderer2D(Manager.Map!);
		_renderer = Manager.MapRenderer;

		_active = false;
		_points = new();
		this.paintBox = paintBox;
	}

	public void Start()
	{
		if (_active) return;
		_active = true;

		_inst = _inst.Clone();
		_points = new() { paintBox.MousePosition, paintBox.MousePosition };

		_inst.Segments.Reset(_points);
		_inst.Opacity = Manager.Settings.Draw_Opacity;
		_inst.IsClosed = false;

		_drawGuide = _inst is AreaInstance area &&
					 (area.Symbol.Width == 0 || area.Symbol.Colour == Colour.Transparent);

		var render = _renderer.RenderPathInstance(_inst).Concat(!_drawGuide ? Enumerable.Empty<IShape>() :
			new IShape[] { new Line {Colour = 0xffff8a00, Points = _points, Width = 1, ZIndex = 999}})
			.ConvertCollection();
		paintBox.Add(_inst.Id, render);
	}

	public void Update()
	{
		if (!_active) return;

		_points[^1] = paintBox.MousePosition;

		_inst.Segments.Reset(_points);
		
		var render = _renderer.RenderPathInstance(_inst).Concat(!_drawGuide ? Enumerable.Empty<IShape>() :
			new IShape[] { new Area {BorderColour = Manager.Settings.Draw_BorderColour, Points = _points, BorderWidth = Manager.Settings.Draw_BorderWidth, ZIndex = Manager.Settings.Draw_BorderZIndex }})
			.ConvertCollection();
		paintBox.Update(_inst.Id, render);
	}

	public void NewPoint()
	{
		if (!_active) return;

		WriteLine("New Point");

		_points.Add(paintBox.MousePosition);

		_inst.Segments.Reset(_points);

		var render = _renderer.RenderPathInstance(_inst).ConvertCollection();
		paintBox.Update(_inst.Id, render);
	}

	public void End()
	{
		if (!_active) return;
		_active = false;

		_points.Add(paintBox.MousePosition);

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

		Manager.Map!.Instances.Add(_inst);

		var render = _renderer.RenderPathInstance(_inst).ConvertCollection();
		paintBox.Update(_inst.Id, render);

		
	}

	public void Complete()
	{
		if (!_active) return;
		_active = false;

		_points.Remove(_points[^1]);
		//_points.Add(ViewManager.MousePosition);

		if (_points[0] == _points[1])
			_points.RemoveAt(1);

		_inst.Segments.Reset(_points);
		_inst.Opacity = 1f;
		_inst.IsClosed = true;

		Manager.Map!.Instances.Add(_inst);

		var render = _renderer.RenderPathInstance(_inst).ConvertCollection();
		paintBox.Update(_inst.Id, render);
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

		paintBox.Remove(_inst.Id);
	}
}

file static class Extension
{
	public static void Reset(this PathCollection pC, IEnumerable<vec2> points)
	{
		pC.Clear();
		pC.Add(new LinearPath(points));
	}
}