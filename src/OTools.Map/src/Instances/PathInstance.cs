using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using OTools.Maps;
using Sunley.Mathematics;

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
	public LineInstance(int layer, LineSymbol symbol, PathCollection segments, bool isClosed)
		: base(layer, symbol, segments, isClosed, 0f, null) { }

	public LineInstance(Guid id, int layer, LineSymbol symbol, PathCollection segments, bool isClosed)
		: base(id, layer, symbol, segments, isClosed, 0f, null) { }
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
			LineSymbol l => new LineInstance(Layer, l, Segments, IsClosed),
			AreaSymbol a => new AreaInstance(Layer, a, Segments, IsClosed, PatternRotation, Holes),
			_ => throw new InvalidOperationException()
		};
	}
}

public sealed class PathCollection : IList<IPath>
{
	private readonly List<IPath> _segments;
	private Dictionary<int, bool> _gaps;

	public PathCollection()
	{
		_segments = new();
		_gaps = new();
	}

	public PathCollection(IEnumerable<vec2> points)
	{
		LinearPath lP = new(points);
		_segments = new() { lP };
        _gaps = new() { { 0, false } };
    }

    #region IList Implementation
    public IPath this[int index] { get => _segments[index]; set => _segments[index] = value; }

	public int Count => _segments.Count;

	public bool IsReadOnly => false;

    public void Add(IPath item)
	{
		_segments.Add(item);
		_gaps.Add(_segments.IndexOf(item), false);
	}

    public void Clear()
	{
		_segments.Clear();
		_gaps.Clear();
	}

    public bool Contains(IPath item)
		=> _segments.Contains(item);

    public void CopyTo(IPath[] array, int arrayIndex)
		=> _segments.CopyTo(array, arrayIndex);

    public IEnumerator<IPath> GetEnumerator()
		=> _segments.GetEnumerator();

    public int IndexOf(IPath item)
		=> _segments.IndexOf(item);

    public void Insert(int index, IPath item)
		=> _segments.Insert(index, item);

    public bool Remove(IPath item)
		=> _segments.Remove(item);

    public void RemoveAt(int index)
		=> _segments.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator()
		=> _segments.GetEnumerator();
    #endregion

	public bool IsGap(IPath path)
	{
		if (!_segments.Contains(path))
			throw new ArgumentException("Path is not in collection");
		int index = _segments.IndexOf(path);

		if (_gaps.TryGetValue(index, out var isGap))
            return isGap;
		return false;	
	}
	public bool SetGap(IPath path)
	{
		if (!_segments.Contains(path))
			return false;
		int index = _segments.IndexOf(path);

		if (_gaps.ContainsKey(index))
			_gaps[index] = true;
		else
			_gaps.Add(index, true);

		return true;
	}

	public IList<vec2> LinearApproximation(int n = 99)
	{
		List<vec2> points = new();
		foreach (IPath seg in _segments)
		{
			points.AddRange(seg switch
			{
				BezierPath b => b.LinearApproximation(n),
				LinearPath l => l,
				_ => throw new InvalidOperationException()
			});
		}

		return points;
	}

	public IEnumerable<vec2> GetPoints()
	{
		List<vec2> points = new();

		foreach (IPath path in _segments)
		{
			switch (path)
			{
				case LinearPath l:
					points.AddRange(l);
					break;
				case BezierPath b:
					foreach (BezierPoint p in b)
					{
						points.Add(p.Anchor);

                        if (p.EarlyControl.IsT0)
                            points.Add(p.EarlyControl.AsT0);

                        if (p.LateControl.IsT0)
                            points.Add(p.LateControl.AsT0);
                    }
					break;
            }
		}

		return points;
	}

	public IEnumerable<BezierPoint> GetBeziers()
	{
		List<BezierPoint> points = new();

		foreach (IPath path in _segments)
		{
			if (path is BezierPath bez)
			{
				foreach (BezierPoint p in bez)
                    points.Add(p);
			}
        }

		return points;
	}
}

//public interface IPath
//{
//	bool IsGap { get; set; }

//	IEnumerable<vec2> GetAllPoints();
//	IEnumerable<vec2> GetAnchorPoints();
//	IEnumerable<vec2> GetControlPoints();
//}


//public struct BezierPath : IPath, IEnumerable<BezierPoint>
//{
//	private readonly List<BezierPoint> _points;

//	public bool IsGap { get; set; }

//	public BezierPoint this[int index]
//	{
//		get => _points[index];
//		set => _points[index] = value;
//	}


//	public BezierPath(IList<BezierPoint>? points = null)
//	{
//		points ??= Array.Empty<BezierPoint>();
//		_points = new(points);

//		IsGap = false;
//	}

//	public IEnumerable<vec2> GetAllPoints()
//		=> _points.SelectMany(point => new[] { point.EarlyControl, point.Anchor, point.LateControl });

//	public IEnumerable<vec2> GetControlPoints()
//		=> _points.SelectMany(point => new[] { point.EarlyControl, point.LateControl });

//	public IEnumerable<vec2> GetAnchorPoints()
//		=> _points.Select(x => x.Anchor);

//	public IEnumerator<BezierPoint> GetEnumerator() => _points.GetEnumerator();
//	IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();
//}

//public struct LinearPath : IPath, IEnumerable<vec2>
//{
//	private readonly List<vec2> _points;

//	public bool IsGap { get; set; }

//	public vec2 this[int index]
//	{
//		get => _points[index];
//		set => _points[index] = value;
//	}

//	public LinearPath(IEnumerable<vec2>? points = null)
//	{
//		points ??= new List<vec2>();
//		_points = new(points);

//		IsGap = false;
//	}

//	public int IndexOf(vec2 v2)
//		=> _points.IndexOf(v2);

//	public IEnumerable<vec2> GetAllPoints()
//		=> _points;

//	public IEnumerable<vec2> GetAnchorPoints()
//		=> _points;

//	public IEnumerable<vec2> GetControlPoints()
//		=> Enumerable.Empty<vec2>();

//	public IEnumerator<vec2> GetEnumerator() => _points.GetEnumerator();
//	IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();
//}