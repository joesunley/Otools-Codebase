using System.Collections;
using OTools.Maps;

namespace OTools.Maps;

public abstract class PathInstance<T> : Instance<T>, PathInstance where T : Symbol
{
	public PathCollection Segments { get; set; }
	public List<PathCollection> Holes { get; set; }

	public bool IsClosed { get; set; }

	public float PatternRotation { get; set; } // Radians

	IPathSymbol PathInstance.Symbol
	{
		get => (IPathSymbol)Symbol;
		set => Symbol = (T)value;
	}

	protected PathInstance(int layer, T symbol, PathCollection segments, bool isClosed, float patternRotation, IEnumerable<PathCollection>? holes = null)
		: base(layer, symbol)
	{
		Segments = segments;
		IsClosed = isClosed;
		PatternRotation = patternRotation;
		Holes = holes?.ToList() ?? new();
	}

	protected PathInstance(Guid id, int layer, T symbol, PathCollection segments, bool isClosed, float patternRotation, IEnumerable<PathCollection>? holes = null)
		: base(id, layer, symbol)
	{
		Segments = segments;
		IsClosed = isClosed;
		PatternRotation = patternRotation;
		Holes = holes?.ToList() ?? new();
	}
}

public sealed class LineInstance : PathInstance<LineSymbol>
{
	public LineInstance(int layer, LineSymbol symbol, PathCollection segments, bool isClosed, float patternRotation, IEnumerable<PathCollection>? holes = null)
		: base(layer, symbol, segments, isClosed, patternRotation, holes) { }

	public LineInstance(Guid id, int layer, LineSymbol symbol, PathCollection segments, bool isClosed, float patternRotation, IEnumerable<PathCollection>? holes = null)
		: base(id, layer, symbol, segments, isClosed, patternRotation, holes) { }
}

public sealed class AreaInstance : PathInstance<AreaSymbol>
{
	public AreaInstance(int layer, AreaSymbol symbol, PathCollection segments, bool isClosed, float patternRotation, IEnumerable<PathCollection>? holes = null)
		: base(layer, symbol, segments, isClosed, patternRotation, holes) { }

	public AreaInstance(Guid id, int layer, AreaSymbol symbol, PathCollection segments, bool isClosed, float patternRotation, IEnumerable<PathCollection>? holes = null)
		: base(id, layer, symbol, segments, isClosed, patternRotation, holes) { }
}

public interface PathInstance : Instance
{
	PathCollection Segments { get; }

	List<PathCollection> Holes { get; }

	bool IsClosed { get; set; }

	public float PatternRotation { get; set; }

	new IPathSymbol Symbol { get; set; }

	public PathInstance Clone()
	{
		return Symbol switch
		{
			LineSymbol l => new LineInstance(Layer, l, Segments, IsClosed, PatternRotation, Holes),
			AreaSymbol a => new AreaInstance(Layer, a, Segments, IsClosed, PatternRotation, Holes),
			_ => throw new InvalidOperationException()
		};
	}
}

public sealed class PathCollection : List<IPathSegment>
{
	public IEnumerable<vec2> GetAllPoints()
	{
		List<vec2> points = new();

		foreach (IPathSegment obj in this)
			points.AddRange(obj.GetAllPoints());

		return points;
	}

	public IEnumerable<vec2> GetAnchorPoints()
	{
		List<vec2> points = new();

		foreach (IPathSegment obj in this)
			points.AddRange(obj.GetAnchorPoints());

		return points;
	}

	public IEnumerable<vec2> GetControlPoints()
	{
		List<vec2> points = new();

		foreach (IPathSegment obj in this)
		{
			if (obj is BezierPath bez)
			{
				points.AddRange(bez.Select(x => x.EarlyControl));
				points.AddRange(bez.Select(x => x.LateControl));
			}
		}

		return points;
	}

	private const float RESOLUTION = 0.05f;

	public IList<vec2> Linearise(float resolution = RESOLUTION)
	{
		List<vec2> points = new();

		foreach (IPathSegment seg in this)
		{
			switch (seg)
			{
				case LinearPath line: points.AddRange(line); break;
				case BezierPath bez:
					{
						for (int i = 1; i < bez.Count(); i++)
						{
							BezierPoint early = bez[i - 1], late = bez[i];

							for (float t = 0f; t <= 1; t += resolution)
							{
								vec2 p0 = vec2.Lerp(early.Anchor, early.LateControl, t);
								vec2 p1 = vec2.Lerp(early.LateControl, late.EarlyControl, t);
								vec2 p2 = vec2.Lerp(late.EarlyControl, late.Anchor, t);

								vec2 d = vec2.Lerp(p0, p1, t);
								vec2 e = vec2.Lerp(p1, p2, t);

								points.Add(vec2.Lerp(d, e, t));
							}
						}
					}
					break;
			}
		}

		return points;
	}
}

public interface IPathSegment
{
	bool IsGap { get; set; }

	IEnumerable<vec2> GetAllPoints();
	IEnumerable<vec2> GetAnchorPoints();
	IEnumerable<vec2> GetControlPoints();
}

public struct BezierPoint
{
	public vec2 Anchor { get; set; }
	public vec2 EarlyControl { get; set; }
	public vec2 LateControl { get; set; }

	public BezierPoint(vec2 anchor, vec2? earlyControl = null, vec2? lateControl = null)
	{
		Anchor = anchor;
		EarlyControl = earlyControl ?? anchor;
		LateControl = lateControl ?? anchor;
	}
}

public struct BezierPath : IPathSegment, IEnumerable<BezierPoint>
{
	private readonly List<BezierPoint> _points;

	public bool IsGap { get; set; }

	public BezierPoint this[int index]
	{
		get => _points[index];
		set => _points[index] = value;
	}


	public BezierPath(IList<BezierPoint>? points = null)
	{
		points ??= Array.Empty<BezierPoint>();
		_points = new(points);

		IsGap = false;
	}

	public IEnumerable<vec2> GetAllPoints()
		=> _points.SelectMany(point => new[] { point.EarlyControl, point.Anchor, point.LateControl }).ToList();

	public IEnumerable<vec2> GetControlPoints()
		=> _points.SelectMany(point => new[] { point.EarlyControl, point.LateControl }).ToList();

	public IEnumerable<vec2> GetAnchorPoints()
		=> _points.Select(x => x.Anchor).ToList();

	public IEnumerator<BezierPoint> GetEnumerator() => _points.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();
}

public struct LinearPath : IPathSegment, IEnumerable<vec2>
{
	private readonly List<vec2> _points;

	public bool IsGap { get; set; }

	public vec2 this[int index]
	{
		get => _points[index];
		set => _points[index] = value;
	}

	public LinearPath(IEnumerable<vec2>? points = null)
	{
		points ??= new List<vec2>();
		_points = new(points);

		IsGap = false;
	}

	public int IndexOf(vec2 v2)
		=> _points.IndexOf(v2);

	public IEnumerable<vec2> GetAllPoints()
		=> _points;

	public IEnumerable<vec2> GetAnchorPoints()
		=> _points;

	public IEnumerable<vec2> GetControlPoints()
		=> Enumerable.Empty<vec2>();

	public IEnumerator<vec2> GetEnumerator() => _points.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();
}