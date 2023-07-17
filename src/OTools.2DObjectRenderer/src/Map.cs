using ComputeSharp;
using OneOf.Types;
using OTools.Maps;
using Sunley.Mathematics;
using System.Data;
using System.Diagnostics;
using static System.Diagnostics.Debug;
namespace OTools.ObjectRenderer2D;

public interface IMapRenderer2D : IVisualRenderer
{
	IEnumerable<IShape> RenderMapObjects(IEnumerable<MapObject> objs);
	IEnumerable<IShape> RenderMapObject(MapObject obj);
	IEnumerable<IShape> RenderPointObject(PointObject obj);
	IEnumerable<IShape> RenderLineObject(LineObject obj);
	IEnumerable<IShape> RenderAreaObject(AreaObject obj);
	IEnumerable<IShape> RenderTextObject(TextObject obj);

	IEnumerable<IShape> RenderSymbols(IEnumerable<Symbol> syms);
	IEnumerable<IShape> RenderSymbol(Symbol sym);
	IEnumerable<IShape> RenderPointSymbol(PointSymbol sym);
	IEnumerable<IShape> RenderPathSymbol(IPathSymbol sym);

	IEnumerable<IShape> RenderInstances(IEnumerable<Instance> insts);
	IEnumerable<IShape> RenderInstance(Instance inst);
	IEnumerable<IShape> RenderPointInstance(PointInstance inst);
	IEnumerable<IShape> RenderPathInstance(PathInstance inst);
	IEnumerable<IShape> RenderTextInstance(TextInstance inst);

	IEnumerable<(Instance, IEnumerable<IShape>)> RenderMap();
}

public class MapRenderer2D : IMapRenderer2D
{
	private Map _activeMap;

	private readonly Dictionary<int, IEnumerable<IShape>> _symbolCache;

	public MapRenderer2D(Map map)
	{
		_activeMap = map;
		_symbolCache = new();
	}

	public IEnumerable<(Instance, IEnumerable<IShape>)> RenderMap()
	{
		List<(Instance, IEnumerable<IShape>)> objs = new();
		foreach (Instance inst in _activeMap.Instances)
			objs.Add((inst, RenderInstance(inst)));
		return objs;
	}

	public IEnumerable<IShape> Render() => RenderMap().Select(x => x.Item2).SelectMany(x => x);

	public IEnumerable<IShape> RenderMapObjects(IEnumerable<MapObject> objs)
	{
		List<IShape> shapes = new();
		foreach (MapObject obj in objs)
			shapes.AddRange(RenderMapObject(obj));
		return shapes;
	}
	public IEnumerable<IShape> RenderMapObject(MapObject obj)
	{
		return obj switch
		{
			PointObject p => RenderPointObject(p),
			LineObject l => RenderLineObject(l),
			AreaObject a => RenderAreaObject(a),
			TextObject t => RenderTextObject(t),
			_ => throw new InvalidOperationException()
		};
	}
	public IEnumerable<IShape> RenderPointObject(PointObject obj)
	{
		float diameter = 2 * (obj.InnerRadius + obj.OuterRadius);

		Circle innerEllipse = new()
		{
			Diameter = 2 * obj.InnerRadius,

			Fill = obj.InnerColour,

			BorderWidth = 0,

			ZIndex = obj.InnerColour.Precedence,
		};

		Circle outerEllipse = new()
		{
			Diameter = diameter,

			BorderWidth = obj.OuterRadius,
			BorderColour = obj.OuterColour,

			ZIndex = obj.OuterColour.Precedence,
		};

		// Prevents unnecessary ellipses
		if (obj.OuterColour == Colour.Transparent)
			return new IShape[] { innerEllipse };
		if (obj.InnerColour == Colour.Transparent)
			return new IShape[] { outerEllipse };
		return new IShape[] { innerEllipse, outerEllipse };
	}
	public IEnumerable<IShape> RenderLineObject(LineObject obj)
	{
		List<IPathSegment> segments = new();

		foreach (IPath seg in obj.Segments)
		{
			segments.Add(seg switch
			{
				LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
				BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
				_ => throw new InvalidOperationException(),
			});
		}

		Path border = new()
		{
			Segments = segments,

			IsClosed = obj.IsClosed,

			BorderColour = obj.Colour,
			BorderWidth = obj.Width,

			ZIndex = obj.Colour.Precedence,
		};

		return new IShape[] { border };

	}
	public IEnumerable<IShape> RenderAreaObject(AreaObject obj)
	{
		List<IPathSegment> segments = new();

		foreach (IPath seg in obj.Segments)
		{
			segments.Add(seg switch
			{
				LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                _ => throw new InvalidOperationException(),
			});
		}

		List<IShape> renders = new();

		Path border = new()
		{
			Segments = segments,

			BorderColour = obj.BorderColour,
			BorderWidth = obj.BorderWidth,

			IsClosed = true,

			ZIndex = obj.BorderColour.Precedence,
		};

		renders.Add(border);

		IList<vec2> poly = obj.Segments.LinearApproximation();
		vec4 rect = poly.AABB();

		var fillObjs = obj.Fill switch
		{
			SolidFill sFill => RenderSolidFill(sFill, rect, true),
			PatternFill pFill => RenderPatternFill(pFill, poly, rect, 0),
			ObjectFill oFill => RenderObjectFill(oFill, poly, rect, true),
			CombinedFill cFill => RenderCombinedFill(cFill, poly, rect, 0, true),
			_ => throw new InvalidOperationException(), // Can't happen
		};

		if (fillObjs.Any())
		{
			Path area = new()
			{
				Segments = segments,

				BorderWidth = obj.BorderWidth,
				BorderColour = obj.BorderColour,

				IsClosed = true,

				Fill = VisualFill(fillObjs, rect),
			};

			renders.Add(area);
		}
		else
		{
			Path area = new()
			{
				Segments = segments,

				BorderWidth = obj.BorderWidth,
				BorderColour = obj.BorderColour,

				IsClosed = true,
			};

			renders.Add(area);
		}

		renders.AddRange(obj.Fill switch
		{
			ObjectFill oFill => RenderObjectFill(oFill, poly, rect, false),
			CombinedFill cFill => RenderCombinedFill(cFill, poly, rect, 0, false),
			_ => throw new Exception(),
		});

		return renders;
	}
	public IEnumerable<IShape> RenderTextObject(TextObject obj)
	{
		Text text = new()
		{
			TopLeft = obj.TopLeft,
			Content = obj.Text,
			Font = obj.Font,
			HorizontalAlignment = obj.HorizontalAlignment,
			Border = (obj.BorderColour, obj.BorderWidth),
			Framing = (obj.FramingColour, obj.FramingWidth),
			ZIndex = obj.Font.Colour.Precedence,
		};

		return new IShape[] { text };
	}

	public IEnumerable<IShape> RenderSymbols(IEnumerable<Symbol> syms)
	{
		List<IShape> shapes = new();
		foreach (Symbol sym in syms)
			shapes.AddRange(RenderSymbol(sym));
		return shapes;

	}
	public IEnumerable<IShape> RenderSymbol(Symbol sym)
	{
		int hash = sym.GetHashCode();
		if (_symbolCache.TryGetValue(hash, out IEnumerable<IShape> value))
			return value;

		var result = sym switch
		{
			PointSymbol p => RenderPointSymbol(p),
			IPathSymbol l => RenderPathSymbol(l),
			// TextSymbol t => Fully rendered at instance
			_ => throw new InvalidOperationException(),
		};

		_symbolCache.Add(hash, result);
		return result;
	}
	public IEnumerable<IShape> RenderPointSymbol(PointSymbol sym)
	{
		int hash = sym.GetHashCode();
		if (_symbolCache.TryGetValue(hash, out IEnumerable<IShape>? value))
			return value;

		var shapes = RenderMapObjects(sym.MapObjects);
		_symbolCache.Add(hash, shapes);
		return shapes;
	}
	public IEnumerable<IShape> RenderPathSymbol(IPathSymbol sym)
	{
		//Note: Majority is implemented in RenderPathInstance(..)

		int hash = sym.GetHashCode();
		if (_symbolCache.TryGetValue(hash, out IEnumerable<IShape>? value))
			return value!;

		Path path = new()
		{
			BorderWidth = sym.Width,
			BorderColour = sym.Colour,

			IsClosed = false,

			ZIndex = sym.Colour.Precedence,
		};

		//TODO: Might not be worth it
		var shapes = new IShape[] { path };
		_symbolCache.Add(hash, shapes);
		return shapes;
	}

	public IEnumerable<IShape> RenderInstances(IEnumerable<Instance> insts)
	{
		List<IShape> shapes = new();
		foreach (Instance inst in insts)
			shapes.AddRange(RenderInstance(inst));
		return shapes;
	}
	public IEnumerable<IShape> RenderInstance(Instance inst)
	{
		return inst switch
		{
			PointInstance p => RenderPointInstance(p),
			LineInstance l => RenderPathInstance(l),
			AreaInstance a => RenderPathInstance(a),
			TextInstance t => RenderTextInstance(t),
			_ => throw new InvalidOperationException(),
		};
	}
	public IEnumerable<IShape> RenderPointInstance(PointInstance inst)
	{
		foreach (IShape el in RenderPointSymbol(inst.Symbol))
		{
			el.Opacity = inst.Opacity;
			el.TopLeft = inst.Centre;

			if (el is Path path && inst.Rotation != 0)
			{
				path.Segments = new(_Utils.RotatePath(path.Segments, inst.Rotation, vec2.Zero)); 

				yield return path;
			} else yield return el;
		}
	}
	public IEnumerable<IShape> RenderPathInstance(PathInstance inst)
	{
		List<IPathSegment> segments = new();
		foreach (IPath seg in inst.Segments)
		{
			segments.Add(seg switch
			{
				LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                _ => throw new InvalidOperationException(),
			});
		}

		List<IPathSegment[]> holes = new();
		foreach (PathCollection coll in inst.Holes)
		{
			List<IPathSegment> hole = new();
			foreach (IPath seg in coll)
			{
				hole.Add(seg switch
				{
					LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                    BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                    _ => throw new InvalidOperationException(),
				});
			}

			holes.Add(hole.ToArray());
		}

		List<IShape> renders = new();

		if (inst.Symbol.Colour != Colour.Transparent)
		{
			Path border = new()
			{
				Segments = segments,
				Holes = holes,

				BorderWidth = inst.Symbol.Width,
				BorderColour = inst.Symbol.Colour,

				IsClosed = inst.IsClosed,

				DashArray = _Utils.CreateDashArray(_Utils.CalculateLengthOfPath(inst.Segments), inst.Symbol.DashStyle, inst.Symbol.Width),

				ZIndex = inst.Symbol.Colour.Precedence,
			};

			renders.Add(border);
		}

		if (inst.Symbol is AreaSymbol aSym)
		{
			var poly = inst.Segments.LinearApproximation();
			vec4 rect = poly.AABB();

			VisualFill visFill = new();

			var fillObjs = aSym.Fill switch
			{
				SolidFill sFill => Enumerable.Empty<IShape>(),
				PatternFill pFill => RenderPatternFill(pFill, poly, rect, inst.PatternRotation),
				ObjectFill oFill => RenderObjectFill(oFill, poly, rect, true),
				CombinedFill cFill => RenderCombinedFill(cFill, poly, rect, inst.PatternRotation, true),
				_ => throw new InvalidOperationException(), // Can't happen
			};

			if (fillObjs.Any())
				visFill = VisualFill(fillObjs, rect);

			Path area = new()
			{
				Segments = segments,
				Holes = holes,

				IsClosed = inst.IsClosed,

				Fill = aSym.Fill is SolidFill s ? s.Colour.HexValue : visFill,

				DashArray = _Utils.CreateDashArray(_Utils.CalculateLengthOfPath(inst.Segments), inst.Symbol.DashStyle, inst.Symbol.Width),
			};

			renders.Add(area);

			renders.AddRange(aSym.Fill switch
			{
				ObjectFill oFill => RenderObjectFill(oFill, poly, rect, false),
				CombinedFill cFill => RenderCombinedFill(cFill, poly, rect, inst.PatternRotation, false),
				_ => Enumerable.Empty<IShape>(),
			});
		}

		if (inst.Symbol.MidStyle.HasMid)
		{
			IEnumerable<vec2> mids = _Utils.CalculateMidPoints(inst.Segments, inst.Symbol.MidStyle);

			foreach (vec2 p in mids)
				renders.AddRange(RenderMapObjects(inst.Symbol.MidStyle.MapObjects)
					.Select(el => { el.TopLeft = p; return el; }));
		}

		return renders.Select(x => { x.Opacity = inst.Opacity; return x; });
	}
	public IEnumerable<IShape> RenderTextInstance(TextInstance inst)
	{
		Text text = new()
		{
			TopLeft = inst.TopLeft,
			Content = inst.Text,
			Font = inst.Symbol.Font,
			HorizontalAlignment = inst.HorizontalAlignment,
			Border = (inst.Symbol.BorderColour, inst.Symbol.BorderWidth),
			Framing = (inst.Symbol.FramingColour, inst.Symbol.FramingWidth),
			Opacity = inst.Opacity,
			ZIndex = inst.Symbol.Font.Colour.Precedence,
		};

		return new IShape[] { text };

	}

	private VisualFill VisualFill(IEnumerable<IShape> shapes, vec4 aabb)
	{
		VisualFill vFill = new()
		{
			Shapes = new(shapes),
			Viewport = aabb,
			TopLeft = aabb.XY,
		};

		return vFill;
	}
	private IEnumerable<IShape> RenderObjectFill(ObjectFill oFill, IEnumerable<vec2> poly, vec4 rect, bool shouldBeClipped)
	{
		if (oFill.IsClipped != shouldBeClipped) // Jank
			return Enumerable.Empty<IShape>();

		vec4 bBox = rect + (-10, -10, 10, 10);

		IEnumerable<vec2> points = oFill switch
		{
			RandomObjectFill roFill => PolygonTools.RandomlySampledRect(bBox, roFill.Spacing),
			SpacedObjectFill soFill => PolygonTools.SpacedSampleRects(poly, (-10, -10, 10, 10), soFill.Spacing, soFill.Rotation),
			_ => throw new InvalidOperationException(), // Can't happen
		};

		if (!oFill.IsClipped)
			points = points.Where(p => PolygonTools.IsPointInPoly(poly, p, rect.XY - (5, 5.0001)));

		List<IShape> renders = new();
		foreach (vec2 p in points)
			renders.AddRange(RenderMapObjects(oFill.Objects)
				.Select(el => { el.TopLeft = p; return el; }));

		return renders;
	}
	private IEnumerable<IShape> RenderPatternFill(PatternFill pFill, IEnumerable<vec2> poly, vec4 rect, float patternRotation)
	{
		float rotation = pFill.Rotation + patternRotation;

		List<(vec4, bool)> lines = new();

		if (rotation == 0f)
			lines = PolygonTools.UnrotatedLines(rect, pFill.ForeSpacing, pFill.BackSpacing).ToList();
		else
		{
			IEnumerable<vec2> rPoly = PolygonTools.Rotate(poly, -rotation, rect.XY);
			vec4 rRect = rPoly.AABB();

			IEnumerable<(vec4, bool)> rLines = PolygonTools.UnrotatedLines(rRect, pFill.ForeSpacing, pFill.BackSpacing);

			foreach ((vec4, bool) rL in rLines)
			{
				vec2[] points = { rL.Item1.XY, rL.Item1.ZW };
				vec2[] rPoints = PolygonTools.Rotate(points, rotation, rect.XY).ToArray();

				lines.Add((new(rPoints[0], rPoints[1]), rL.Item2));
			}
		}

		List<IShape> renders = new();
		foreach ((vec4, bool) line in lines)
		{
			Line l = new()
			{
				Points = { line.Item1.XY, line.Item1.ZW },
				Colour = line.Item2 ? pFill.ForeColour : pFill.BackColour,
				Width = line.Item2 ? pFill.ForeSpacing : pFill.BackSpacing,
			};

			renders.Add(l);
		}

		return renders;
	}
	private IEnumerable<IShape> RenderCombinedFill(CombinedFill cFill, IList<vec2> poly, vec4 rect, float patternRotation, bool shouldBeClipped)
	{
		bool isObjectDone = false, isSolidDone = false;

		List<IShape> renders = new();
		foreach (var fill in cFill.Fills)
		{
			switch (fill)
			{
				case ObjectFill oFill:
					Assert(!isObjectDone, "Should only have 1 object fill");

					renders.AddRange(RenderObjectFill(oFill, poly, rect, shouldBeClipped)); // Might need to add patternRotation
					isObjectDone = true;
					break;
				case PatternFill pFill:
					renders.AddRange(RenderPatternFill(pFill, poly, rect, patternRotation));
					break;
				case SolidFill sFill:
					Assert(!isSolidDone, "Should only have 1 solid fill");

					renders.AddRange(RenderSolidFill(sFill, rect, shouldBeClipped));
					isSolidDone = true;
					break;
			}
		}

		return renders;
	}
	private IEnumerable<IShape> RenderSolidFill(SolidFill sFill, vec4 rect, bool shouldBeClipped)
	{
		if (!shouldBeClipped)
			return Enumerable.Empty<IShape>();

		rect += (-5, -5, 5, 5);

		Rectangle r = new()
		{
			TopLeft = rect.XY,
			Size = rect.ZW - rect.XY,

			Fill = sFill.Colour,

			ZIndex = sFill.Colour.Precedence,
		};

		Debug.WriteLine(rect);

		return new IShape[] { r };
	}
}

public class WireframeMapRenderer2D : IMapRenderer2D
{
	public static float LineWidth { get; set; }
	public static float PointRadius { get; set; }

	private Map _activeMap;

	// Need to add way to update for changed symbols.
	// Could just redo the calculation for every symbol every time there is
	// an update but is inefficient
	private readonly Dictionary<Guid, Colour> _cachedPrimaryColours;

	public WireframeMapRenderer2D(Map map)
	{
		_activeMap = map;

		_cachedPrimaryColours = new();

		foreach (var kvp in CalculatePrimaryColours(_activeMap.Symbols))
			_cachedPrimaryColours.TryAdd(kvp.Item1, kvp.Item2);

		// MapObject thingying -> maybe of each point symbol?
	}

	public IEnumerable<(Instance, IEnumerable<IShape>)> RenderMap()
	{
		List<(Instance, IEnumerable<IShape>)> objs = new();
		foreach (Instance inst in _activeMap.Instances)
			objs.Add((inst, RenderInstance(inst)));
		return objs;
	}
	
	public IEnumerable<IShape> Render() => RenderMap().SelectMany(x => x.Item2);

	public IEnumerable<IShape> RenderMapObjects(IEnumerable<MapObject> objs)
	{
		List<IShape> shapes = new();
		foreach (MapObject obj in objs)
			shapes.AddRange(RenderMapObject(obj));
		return shapes;
	}
	public IEnumerable<IShape> RenderMapObject(MapObject obj)
	{
		// Should not allow rendering without calculating primary colour
		Assert(_cachedPrimaryColours.ContainsKey(obj.Id));

		return obj switch
		{
			PointObject p => RenderPointObject(p),
			LineObject l => RenderLineObject(l),
			AreaObject a => RenderAreaObject(a),
			TextObject t => RenderTextObject(t),
			_ => throw new InvalidOperationException()
		};
	}
	public IEnumerable<IShape> RenderPointObject(PointObject obj)
	{
		Colour col = _cachedPrimaryColours[obj.Id];

		Circle output = new()
		{
			Diameter = 2 * PointRadius,

			Fill = col,

			BorderWidth = 0,

			ZIndex = col.Precedence,
		};

		return new IShape[] { output };
	}
	public IEnumerable<IShape> RenderLineObject(LineObject obj)
	{
		Colour col = _cachedPrimaryColours[obj.Id];

		List<IPathSegment> segments = new();

		foreach (IPath seg in obj.Segments)
		{
			segments.Add(seg switch
			{
				LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                _ => throw new InvalidOperationException(),
			});
		}

		Path path = new()
		{
			Segments = segments,

			BorderWidth = LineWidth,
			BorderColour = col,

			IsClosed = obj.IsClosed,

			ZIndex = col.Precedence,
		};

		return new IShape[] { path };
	}
	public IEnumerable<IShape> RenderAreaObject(AreaObject obj)
	{
		Colour col = _cachedPrimaryColours[obj.Id];

		List<IPathSegment> segments = new();

		foreach (IPath seg in obj.Segments)
		{
			segments.Add(seg switch
			{
				LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                _ => throw new InvalidOperationException(),
			});
		}

		Path path = new()
		{
			Segments = segments,

			BorderWidth = LineWidth,
			BorderColour = col,

			IsClosed = true,

			ZIndex = col.Precedence,
		};

		return new IShape[] { path };
	}
	public IEnumerable<IShape> RenderTextObject(TextObject obj)
	{
		throw new NotImplementedException();
	}
	
	public IEnumerable<IShape> RenderSymbols(IEnumerable<Symbol> syms)
	{
		List<IShape> shapes = new();
		foreach (Symbol sym in syms)
			shapes.AddRange(RenderSymbol(sym));
		return shapes;
	}
	public IEnumerable<IShape> RenderSymbol(Symbol sym)
	{
		// Should not allow rendering without calculating primary colour
		Assert(_cachedPrimaryColours.ContainsKey(sym.Id));

		return sym switch
		{
			PointSymbol p => RenderPointSymbol(p),
			IPathSymbol l => RenderPathSymbol(l),
			_ => throw new InvalidOperationException(),
		};
	}
	public IEnumerable<IShape> RenderPointSymbol(PointSymbol sym)
	{
		Colour col = _cachedPrimaryColours[sym.Id];

		Circle output = new()
		{
			Diameter = 2 * PointRadius,

			Fill = col,

			BorderWidth = 0,
			
			ZIndex = col.Precedence,
		};

		return new IShape[] { output };
	}
	public IEnumerable<IShape> RenderPathSymbol(IPathSymbol sym)
	{
		Colour col = _cachedPrimaryColours[sym.Id];

		Path path = new()
		{
			BorderWidth = LineWidth,
			BorderColour = col,

			IsClosed = false,

			ZIndex = col.Precedence,
		};

		return new IShape[] { path };
	}
	
	public IEnumerable<IShape> RenderInstances(IEnumerable<Instance> insts)
	{
		List<IShape> shapes = new();
		foreach (Instance inst in insts)
			shapes.AddRange(RenderInstance(inst));
		return shapes;
	}
	public IEnumerable<IShape> RenderInstance(Instance inst)
	{
		return inst switch
		{
			PointInstance p => RenderPointInstance(p),
			LineInstance l => RenderPathInstance(l),
			AreaInstance a => RenderPathInstance(a),
			TextInstance t => RenderTextInstance(t),
			_ => throw new InvalidOperationException(),
		};
	}
	public IEnumerable<IShape> RenderPointInstance(PointInstance inst)
	{
		var els = RenderPointSymbol(inst.Symbol).ToArray();
		Assert(els.Length == 1 && els[0] is Circle);

		Circle e = (Circle)els[0];

		e.TopLeft = inst.Centre;

		return new IShape[] { e };
	}
	public IEnumerable<IShape> RenderPathInstance(PathInstance inst)
	{
		List<IPathSegment> segments = new();
		foreach (IPath seg in inst.Segments)
		{
			segments.Add(seg switch
			{
				LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                _ => throw new InvalidOperationException(),
			});
		}

		List<IPathSegment[]> holes = new();
		foreach (PathCollection coll in inst.Holes)
		{
			List<IPathSegment> hole = new();
			foreach (IPath seg in coll)
			{
				hole.Add(seg switch
				{
					LinearPath linear => new PolyLineSegment { Points = linear.ToList() },
                    BezierPath bezier => new PolyBezierSegment { Points = new(bezier.AsCubicBezier()) },
                    _ => throw new InvalidOperationException(),
				});
			}

			holes.Add(hole.ToArray());
		}

		var els = RenderPathSymbol(inst.Symbol).ToArray();
		Assert(els.Length == 1 && els[0] is Path);

		Path p = (Path)els[0];

		p.Segments = segments;
		p.Holes = holes;
		p.IsClosed = inst.IsClosed;

		return new IShape[] { p };
	}
	public IEnumerable<IShape> RenderTextInstance(TextInstance inst)
	{
		throw new NotImplementedException();

		// Display bounding box -> not sure how to yet
	}

	public static IEnumerable<(Guid, Colour)> CalculatePrimaryColours(IEnumerable<Symbol> syms)
	{
		foreach (Symbol sym in syms)
		{
			switch (sym)
			{
				case PointSymbol p:
					break;
				case LineSymbol l:
					yield return (l.Id, l.Colour); break;
				case AreaSymbol a:
					if (a.Colour != Colour.Transparent)
						yield return (a.Id, a.Colour); break;
					// Lot of work needs doing
			}
		}
	}
	public static IEnumerable<(Guid, Colour)> CalculatePrimaryColours(IEnumerable<MapObject> syms)
	{
		throw new NotImplementedException();
	}
}

internal static partial class _Utils
{
	public static PolyBezierSegment CreateBezierSegment(BezierPath path)
	{
		return new() { Points = new(path.AsCubicBezier()) };
	}

	public static double[] CreateDashArray(float lineLength, DashStyle dashStyle, float lineWidth)
	{
		if (!dashStyle.HasDash)
			return Array.Empty<double>();

		if (dashStyle.GroupSize > 1)
		{
			float minLen = ((dashStyle.DashLength + dashStyle.GapLength) * dashStyle.GroupSize) - dashStyle.GapLength + dashStyle.GroupGapLength;

			if (lineLength < minLen)
				return Array.Empty<double>();

			lineLength += dashStyle.GroupGapLength;

			float maxSingleDash = (((dashStyle.DashLength * 1.2f) + dashStyle.GapLength) * dashStyle.GroupSize) - dashStyle.GapLength + dashStyle.GroupGapLength;
			float minTimes = lineLength / maxSingleDash;

			float dashCount = MathF.Round(minTimes);
			float combinedGroupSize = lineLength / dashCount;

			float combinedDashSize = combinedGroupSize - dashStyle.GroupGapLength;
			float individualDashSize = (combinedDashSize + dashStyle.GapLength) / dashStyle.GroupSize;

			float dashSize = individualDashSize - dashStyle.GapLength;

			List<double> array = new();

			for (int i = 0; i < dashStyle.GroupSize - 1; i++)
			{
				array.Add(dashSize / lineWidth);
				array.Add(dashStyle.GapLength / lineWidth);
			}
			array.Add(dashSize / lineWidth);
			array.Add(dashStyle.GroupGapLength / lineWidth);

			return array.ToArray();
		}
		else
		{
			float minLen = dashStyle.GapLength + (1.6f * dashStyle.DashLength);
			if (lineLength < minLen)
				return Array.Empty<double>(); // No Dash

			lineLength += dashStyle.GapLength;

			float maxSingleDash = dashStyle.GapLength + (1.2f * dashStyle.DashLength);
			float minTimes = lineLength / maxSingleDash;

			float dashCount = MathF.Round(minTimes); // Int
			float combinedDashSize = lineLength / dashCount;

			float dashSize = combinedDashSize - dashStyle.GapLength;

			return new double[] { dashSize / lineWidth, dashStyle.GapLength / lineWidth };
		}
	}

	public static float CalculateLengthOfPath(PathCollection pC)
	{
		float totalLen = 0f;

		foreach (IPath seg in pC)
		{
			switch (seg)
			{
				case LinearPath line:
					totalLen += line.Length();
					break;
				case BezierPath bezier:
					totalLen += bezier.LinearApproximation().Length();
					break;
			}
		}

		return totalLen;
	}

	public static IEnumerable<vec2> CalculateMidPoints(PathCollection pC, MidStyle midStyle)
	{
		List<List<vec2>> paths = new();
		List<vec2> points = new();

		foreach (IPath seg in pC)
		{

			if (pC.IsGap(seg))
			{
				paths.Add(points);
				points = new();

				continue;
			}

			points.AddRange(seg switch
			{
				LinearPath linear => linear,
				BezierPath bezier => bezier.LinearApproximation(),
				_ => throw new InvalidOperationException("Unknown Path Type"),
			});

			paths.Add(points);
			points = new();
		}

		if (paths.Count == 0)
			return CalculateMidPoints(points, midStyle);

		paths.Add(points);

		List<vec2> midPoints = new();
		foreach (List<vec2> p in paths)
			midPoints.AddRange(CalculateMidPoints(p, midStyle));
		return midPoints;
	}

	public static IEnumerable<vec2> CalculateMidPoints(IList<vec2> points, MidStyle midStyle)
	{
		if (points.Count <= 1)
			return Enumerable.Empty<vec2>();

		float spacing = midStyle.GapLength;

		float modInitOff = midStyle.InitialOffset + midStyle.GapLength / 2; // HalfLength on all
		float modEndOff = midStyle.EndOffset - (midStyle.GapLength / 2); // Remove to shift too right .5 spacing

		bool initOffDone = false;

		while (!initOffDone)
		{
			float firstSegLine = vec2.Mag(points[0], points[1]);
			if (firstSegLine > modInitOff)
			{
				vec2 dir = (points[1] - points[0]).Normalise();
				vec2 offset = dir * modInitOff;
				points[0] += offset;

				initOffDone = true;
			}
			else
			{
				points.RemoveAt(0);
				modInitOff -= firstSegLine;

				if (points.Count <= 1)
					return Enumerable.Empty<vec2>();
			}
		}

		bool endOffDone = false;

		while (!endOffDone)
		{
			float lastSegLine = vec2.Mag(points[^1], points[^2]);
			if (lastSegLine > modEndOff)
			{
				vec2 dir = (points[^2] - points[^1]).Normalise();
				vec2 offset = dir * modEndOff;
				points[^1] += offset;

				endOffDone = true;
			}
			else
			{
				points.RemoveAt(points.Count - 1);
				modEndOff -= lastSegLine;
			}
		}

		float len = points.Length();

		float actualCount = len / spacing;
		int count = (int)MathF.Round(actualCount);

		float usableSpacing = len / count;

		if (spacing * 0.8f > usableSpacing || spacing * 1.2f < usableSpacing)
			return Enumerable.Empty<vec2>();
			//throw new Exception(); // Doesn't Work?

		List<vec2> mids = new();
		float currDistance = 0f;

		while (currDistance < len)
		{
			vec2 p = _Utils.PointAtDistance(points.ToList(), currDistance);
			mids.Add(p);
			currDistance += usableSpacing;
		}

		if (points.Count > 2 && mids.Count >= 1 && DoesPointExceedSegment((points[^1], points[^2]), mids[^1]))
			mids.RemoveAt(mids.Count - 1);

		return mids;
	}

	public static vec2 PointAtDistance(IList<vec2> v2s, float distance)
	{
		float totalLen = 0f;
		for (int i = 1; i < v2s.Count; i++)
		{
			float segLen = (v2s[i] - v2s[i - 1]).Mag();

			if (totalLen + segLen >= distance)
			{
				float t = (distance - totalLen) / segLen;
				return vec2.Lerp(v2s[i - 1], v2s[i], t);
			}
			totalLen += segLen;
		}

		return v2s.Last();
	}

	public static bool DoesPointExceedSegment((vec2, vec2) seg, vec2 point)
	{
		vec2 segDir = (seg.Item2 - seg.Item1).Normalise();
		vec2 pointDir = (point - seg.Item1).Normalise();

		return segDir != pointDir;
	}

	public static double RandomRange(double min, double max, Random? rnd = null)
	{
		rnd ??= new Random();

		return rnd.NextDouble() * (max - min) + min;
	}

	// TODO: Remove any possibility of collision with points on the polygon, as cause missed/extra points
	// Might have fixed using very slight difference between topLeft(offset) but not sure
	public static bool IsPointInPolygon(IList<vec2> polygon, vec2 topLeft, vec2 point)
	{
		vec4 line = (topLeft, point);

		int intersections = 0;
		for (int i = 0; i < polygon.Count; i++)
		{
			vec4 segment = i == 0 ?
				(polygon[^1], polygon[0]) : (polygon[i - 1], polygon[i]);

			if (DoLinesIntersect(line, segment))
				intersections++;
		}

		return intersections % 2 == 1;
	}

	public static bool DoLinesIntersect(vec4 l1, vec4 l2)
	{
		int o1 = Orientation(l1.XY, l1.ZW, l2.XY),
			o2 = Orientation(l1.XY, l1.ZW, l2.ZW),
			o3 = Orientation(l2.XY, l2.ZW, l1.XY),
			o4 = Orientation(l2.XY, l2.ZW, l1.ZW);

		if (o1 != o2 && o3 != o4)
			return true;

		if (o1 == 0 && OnSegment(l1.XY, l2.XY, l1.ZW)) return true;
		if (o2 == 0 && OnSegment(l1.XY, l1.ZW, l1.ZW)) return true;
		if (o3 == 0 && OnSegment(l2.XY, l1.XY, l2.ZW)) return true;
		if (o4 == 0 && OnSegment(l2.XY, l1.ZW, l2.ZW)) return true;

		return false;
	}

	public static int Orientation(vec2 a, vec2 b, vec2 c)
	{
		float val = ((b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y));

		if (val == 0) return 0;  // colinear

		return (val > 0) ? 1 : 2; // clock or counterclock wise
	}

	public static bool OnSegment(vec2 p, vec2 q, vec2 r)
	{
		return q.X <= MathF.Max(p.X, r.X) && q.X >= MathF.Min(p.X, r.X) &&
			   q.Y <= MathF.Max(p.Y, r.Y) && q.Y >= MathF.Min(p.Y, r.Y);
	}

	public static bool IsInBounds(vec4 bounds, vec2 p)
	{
		vec2 tL = bounds.XY,
			bR = bounds.ZW;

		return p.X >= tL.X && p.X <= bR.X && p.Y >= tL.Y && p.Y <= bR.Y;
	}

	public static vec2 PointOfIntersectionOfTwoLines(vec4 l1, vec4 l2)
	{
		// Check if parallel
		if ((l1.ZW - l1.XY).Normalise() == (l2.ZW - l2.XY).Normalise())
			throw new DivideByZeroException("Lines are parallel");

		// Get start/end points are the same
		if (l1.XY == l2.XY || l1.XY == l2.ZW)
			return l1.XY;
		if (l1.ZW == l2.XY || l1.ZW == l2.ZW)
			return l1.ZW;

		// Get the intersection point

		vec2 p = l1.XY, r = l1.ZW - l1.XY,
			 q = l2.XY, s = l2.ZW - l2.XY;

		float rxs = Cross(r, s), qpxr = Cross(q - p, r);

		if (rxs == 0 && qpxr == 0)
			throw new Exception("Lines are collinear");

		if (rxs == 0 && qpxr != 0)
			throw new Exception("Lines are parallel");

		float t = Cross(q - p, s) / rxs,
			  u = qpxr / rxs;

		if (rxs != 0 && 0 <= t && t <= 1 && 0 <= u && u <= 1)
			return p + r * t;

		throw new Exception("Lines do not intersect");
	}

	public static bool TryPointOfIntersectionOfTwoLines(vec4 l1, vec4 l2, out vec2 output)
	{
		try
		{
			output = PointOfIntersectionOfTwoLines(l1, l2);
			return true;
		}
		catch
		{
			output = vec2.Zero;
			return false;
		}
	}

	public static float Cross(vec2 a, vec2 b) => (a.X * b.Y) - (a.Y * b.X);

	public static vec4 BoundingBox(this IEnumerable<vec2> v2s)
	{
		vec2 topLeft = vec2.MaxValue,
			 bottomRight = vec2.MinValue;

		foreach (vec2 p in v2s)
		{
			topLeft = vec2.Min(topLeft, p);
			bottomRight = vec2.Max(bottomRight, p);
		}

		return new(topLeft, bottomRight);
	}

	public static IEnumerable<IPathSegment> RotatePath(IEnumerable<IPathSegment> path, float rotation, vec2? rotationCentre = null)
	{
		List<IPathSegment> newSegments = new();

		foreach (IPathSegment seg in path)
		{
			newSegments.Add(seg switch
			{
				PolyLineSegment pls => new PolyLineSegment() { Points = PolygonTools.Rotate(pls.Points, rotation, rotationCentre).ToList() },
				PolyBezierSegment pbs => RotateBezier(pbs, rotation, rotationCentre),
				_ => throw new Exception(), // Can't happen
			});
		}

		return newSegments;
	}

	public static PolyBezierSegment RotateBezier(PolyBezierSegment bez, float rotation, vec2? rotationCentre = null)
	{
		//PolyBezierSegment newBez = new();

		//foreach (var point in bez.Points)
		//{
		//	vec2[] ps = { point.Anchor, point.EarlyControl.AsT0, point.LateControl.AsT0 };
		//	vec2[] rotated = PolygonTools.Rotate(ps, rotation, rotationCentre).ToArray();

		//	newBez.Points.Add(new BezierPoint(rotated[0], rotated[1], rotated[2]));
		//}

		//return newBez;

		throw new NotImplementedException();
	}
}

public static class PolygonTools
{
	public static IEnumerable<vec2> Intersect(IEnumerable<vec2> poly1, IEnumerable<vec2> poly2) => throw new NotImplementedException();
	public static IEnumerable<vec2> Union(IEnumerable<vec2> poly1, IEnumerable<vec2> poly2) => throw new NotImplementedException();
	public static IEnumerable<vec2> Difference(IEnumerable<vec2> poly1, IEnumerable<vec2> poly2) => throw new NotImplementedException();
	public static IEnumerable<vec2> Xor(IEnumerable<vec2> poly1, IEnumerable<vec2> poly2) => throw new NotImplementedException();

	// Needs a lot of optimising / needs rewriting
	public static IEnumerable<vec2> PoissonDiscSampling(IEnumerable<vec2> poly, float spacing, IEnumerable<IEnumerable<vec2>>? holes = null)
	{
		IList<vec2> polyArr = poly.ToArray();
		vec4 rect = polyArr.AABB();

		IEnumerable<vec2> points = RandomlySampledRect(rect, spacing);

		IEnumerable<vec2> validPoints = points.Where(p => IsPointInPoly(polyArr, p, rect.XY - (5, 5.0001)));

		if (holes is null) return validPoints;

		foreach (IEnumerable<vec2> hole in holes)
			validPoints = validPoints.Where(p => !IsPointInPoly(hole, p, rect.XY - (5, 5.0001)));

		return validPoints;
	}

	//	https://www.github.com/SebLague/Poisson-Disc-Sampling
	//
	//	MIT License
	//
	//	Copyright (c) 2020 Sebastian Lague
	//
	//	Permission is hereby granted, free of charge, to any person obtaining a copy
	// 	of this software and associated documentation files (the "Software"), to deal
	// 	in the Software without restriction, including without limitation the rights
	//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	//	copies of the Software, and to permit persons to whom the Software is
	// 	furnished to do so, subject to the following conditions:
	//
	//	The above copyright notice and this permission notice shall be included in all
	// 	copies or substantial portions of the Software.
	//
	// 	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	// 	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	// 	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	// 	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	// 	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	//	SOFTWARE.

	public static IEnumerable<vec2> RandomlySampledRect(vec4 rect, float radius, int k = 30)
	{
		// k => Number of samples before rejection

		vec2 zeroedRect = rect.ZW - rect.XY;
		float cellSize = radius / MathF.Sqrt(2);

		int[,] grid = new int[(int)MathF.Ceiling(zeroedRect.X / cellSize), (int)MathF.Ceiling(zeroedRect.Y / cellSize)];

		List<vec2> points = new(), spawnPoints = new();
		Random rnd = new();

		spawnPoints.Add(zeroedRect / 2);
		while (spawnPoints.Count > 0)
		{
			int spawnIndex = rnd.Next(0, spawnPoints.Count);
			vec2 spawnCentre = spawnPoints[spawnIndex];

			bool candidateAccepted = false;
			for (int i = 0; i < k; i++)
			{
				float angle = (float)rnd.NextDouble() * MathF.PI * 2;
				vec2 dir = (MathF.Sin(angle), MathF.Cos(angle));

				vec2 candidate = spawnCentre + dir * (float)rnd.Next(radius, 2 * radius);
				if (IsValid(candidate, zeroedRect, cellSize, radius, points, grid))
				{
					points.Add(candidate);
					spawnPoints.Add(candidate);
					grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;

					candidateAccepted = true;
					break;
				}
			}

			if (!candidateAccepted)
				spawnPoints.RemoveAt(spawnIndex);
		}

		return points.Select(p => p + rect.XY);
	}

	private static bool IsValid(vec2 candidate, vec2 rect, float cellSize, float radius, IList<vec2> points,
								int[,] grid)
	{
		if (candidate.X >= 0 && candidate.X < rect.X &&
			candidate.Y >= 0 && candidate.Y < rect.Y)
		{
			int cellX = (int)(candidate.X / cellSize),
				cellY = (int)(candidate.Y / cellSize);
			int searchStartX = Math.Max(0, cellX - 2),
				searchEndX = Math.Min(cellX + 2, grid.GetLength(0) - 1);
			int searchStartY = Math.Max(0, cellY - 2),
				searchEndY = Math.Min(cellY + 2, grid.GetLength(1) - 1);

			for (int x = searchStartX; x <= searchEndX; x++)
				for (int y = searchStartY; y <= searchEndY; y++)
				{
					int pointIndex = grid[x, y] - 1;
					if (pointIndex != -1)
					{
						float sqrDst = MathF.Pow((candidate - points[pointIndex]).Mag(), 2);
						if (sqrDst < radius * radius)
							return false;
					}
				}

			return true;
		}

		return false;
	}

	private static double Next(this Random rnd, double min, double max)
		=> rnd.NextDouble() * (max - min) + min;

	public static IEnumerable<vec2> SpacedSampling(IEnumerable<vec2> poly, vec2 spacing, float rotation, IEnumerable<IEnumerable<vec2>>? holes = null)
	{
		vec4 aabb = poly.AABB() + (-spacing.X / 2, -spacing.Y / 2, spacing.X / 2, spacing.Y / 2);

		if (rotation == 0f)
			return SpacedSampledRect(aabb, spacing)
				.Where(x => IsPointInPoly(poly, aabb.XY - (5, 5.0001), x));

		// if rotated
		var rPoly = Rotate(poly, -rotation, aabb.XY);
		vec4 rBBox = rPoly.AABB() + (-spacing.X / 2, -spacing.Y / 2, spacing.X / 2, spacing.Y / 2);

		var points = SpacedSampledRect(rBBox, spacing);

		return Rotate(points, rotation, aabb.XY)
			.Where(x => IsPointInPoly(poly, aabb.XY - (5, 5.0001), x));
	}

	public static IEnumerable<vec2> SpacedSampledRect(vec4 rect, vec2 spacing)
	{
		vec2 curr = rect.XY;

		List<vec2> points = new();
		while (curr.X < rect.Z)
		{
			while (curr.Y < rect.W)
			{
				points.Add(curr);

				curr.Y += spacing.Y;
			}

			curr.X += spacing.X;
			curr.Y = rect.Y;
		}

		return points;
	}

	public static IEnumerable<vec2> Rotate(IEnumerable<vec2> input, float rotation, vec2? rotationCentre = null)
	{
		// Rotation in radians (or is it degrees?)
		rotation %= MathF.PI * 2;

		Assert(rotation != 0, "rotation == 0");

		rotationCentre ??= input.AABB().Midpoint();

		foreach (vec2 p in input)
		{
			vec2 translated = p - rotationCentre.Value;

			float x = translated.X * MathF.Cos(rotation) - translated.Y * MathF.Sin(rotation),
				  y = translated.X * MathF.Sin(rotation) + translated.Y * MathF.Cos(rotation);
			yield return (x, y) + rotationCentre.Value;
		}
	}

	private static vec2 Midpoint(this vec4 v4) => (v4.XY + v4.ZW) / 2;

	public static IEnumerable<vec2> FilterOutside(IEnumerable<vec2> poly, IEnumerable<vec2> points) => throw new NotImplementedException();

	public static bool IsPointInPoly(IEnumerable<vec2> poly, vec2 point, vec2? samplePoint = null)
	{
		vec2 sP = samplePoint ?? (poly.AABB().XY - (1, 1.0001));
		vec4 line = (sP, point);
		ReadOnlySpan<vec2> span = poly.ToArray();

		int intersections = 0;
		for (int i = 0; i < span.Length; i++)
		{
			vec4 segment = (i == 0) ? (span[^1], span[0]) : (span[i - 1], span[i]);

			if (DoLinesIntersect(line, segment))
				intersections++;
		}

		return intersections % 2 == 1;
	}

	private static bool DoLinesIntersect(vec4 l1, vec4 l2)
	{
		int o1 = Orientation(l1.XY, l1.ZW, l2.XY),
			o2 = Orientation(l1.XY, l1.ZW, l2.ZW),
			o3 = Orientation(l2.XY, l2.ZW, l1.XY),
			o4 = Orientation(l2.XY, l2.ZW, l1.ZW);

		if (o1 != o2 && o3 != o4)
			return true;

		if (o1 == 0 && OnSegment(l1.XY, l2.XY, l1.ZW)) return true;
		if (o2 == 0 && OnSegment(l1.XY, l1.ZW, l1.ZW)) return true;
		if (o3 == 0 && OnSegment(l2.XY, l1.XY, l2.ZW)) return true;
		if (o4 == 0 && OnSegment(l2.XY, l1.ZW, l2.ZW)) return true;

		return false;
	}
	private static int Orientation(vec2 a, vec2 b, vec2 c)
	{
		float val = ((b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y));

		if (val == 0) return 0;  // colinear

		return (val > 0) ? 1 : 2; // clock or counterclock wise
	}
	private static bool OnSegment(vec2 p, vec2 q, vec2 r)
	{
		return q.X <= MathF.Max(p.X, r.X) && q.X >= MathF.Min(p.X, r.X) &&
			   q.Y <= MathF.Max(p.Y, r.Y) && q.Y >= MathF.Min(p.Y, r.Y);
	}

	public static IEnumerable<(vec4, bool)> UnrotatedLines(vec4 rect, float foreSpacing, float backSpacing)
	{
		float currY = rect.Y;

		currY += foreSpacing / 2;

		float spacing = foreSpacing / 2 + backSpacing / 2;

		while (currY < rect.W)
		{
			yield return ((rect.X, currY, rect.Z, currY), true);
			currY += spacing;

			yield return ((rect.X, currY, rect.Z, currY), false);
			currY += spacing;
		}
	}

	public static vec4 AABB(this IEnumerable<vec2> v2s)
	{
		vec2 topLeft = vec2.MaxValue,
			 bottomRight = vec2.MinValue;

		foreach (vec2 p in v2s)
		{
			topLeft = vec2.Min(topLeft, p);
			bottomRight = vec2.Max(bottomRight, p);
		}

		return new(topLeft, bottomRight);
	}

	public static IEnumerable<vec2> SpacedSampleRects(IEnumerable<vec2> poly, vec4 delta, vec2 spacing, float rotation)
	{
		vec4 aabb = poly.AABB() + delta;

		if (rotation == 0f)
			return SpacedSampledRect(aabb, spacing);

		// If rotated
		var rPoly = Rotate(poly, -rotation, aabb.XY);
		vec4 rbBox = rPoly.AABB() + delta;

		var points = SpacedSampledRect(rbBox, spacing);

		return Rotate(points, rotation, aabb.XY);
	}

	public static float Length(this IEnumerable<vec2> v2s)
	{
		ReadOnlySpan<vec2> span = v2s.ToArray();

		float tLen = 0f;
		for (int i = 1; i < span.Length; i++)
			tLen += vec2.Mag(span[i - 1], span[i]);

		return tLen;
	}
}