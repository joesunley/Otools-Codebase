using Avalonia.Input;
using OneOf.Types;
using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using ownsmtp.logging;

namespace OTools.MapMaker;

public class MapEdit
{
    private PointEdit? pointEdit;
    private PathEdit? pathEdit;

    public bool IsActive { get; private set; }
    private Active active;

    private Instance? selectedInstance;

    private readonly PaintBox paintBox;

    public MapEdit()
    {
        Manager.ActiveToolChanged += args =>
        {
            IsActive = (args == Tool.Edit);

            if (!IsActive && selectedInstance != null)
                Deselect();
        };

        Assert(Manager.PaintBox != null);
        paintBox = Manager.PaintBox!;

        paintBox.PointerReleased += (_, args) => MouseUp(args.InitialPressMouseButton, args.KeyModifiers);
        paintBox.PointerPressed += (_, args) => MouseDown(args);
        paintBox.MouseMoved += MouseMove;
        paintBox.KeyUp += (_, args) => KeyUp(args.Key);
    }

    public void MouseDown(PointerPressedEventArgs args)
    {
        if (!IsActive) return;

        if (args.GetCurrentPoint(paintBox.canvas).Properties.IsLeftButtonPressed)
            pathEdit?.LMouseDown(); 
    }

    public void MouseUp(MouseButton mouse, KeyModifiers modifiers)
    {
        if (!IsActive) return;

        switch (mouse)
        {
            case MouseButton.Left:
            {

                if (selectedInstance is null)
                    SelectInstance12();

                if (modifiers.HasFlag(KeyModifiers.Control))
                    pathEdit?.CtrlClick();

                pathEdit?.LMouseUp();
            } break;
        }
    }

    public void MouseMove(MouseMovedEventArgs args)
    {
        if (!IsActive) return;

        if (args.Properties.IsLeftButtonPressed)
        {
            pointEdit?.Move();
            pathEdit?.MovePoint();
        }
    }

    public void KeyUp(Key key)
    {
        if (!IsActive) return;

        switch (key)
        {
            case Key.Delete:
                pointEdit?.Delete();
                break;
            case Key.Escape:
                Deselect();
                break;
        }
    }

    private void SelectInstance()
    {
        var (instance, dist) = _Utils.NearestInstance(Manager.Map!.Instances, paintBox.MousePosition);

        if (instance is null) return;   

        if (dist < Manager.Settings.Select_ObjectTolerance)
        {
            Deselect();

            selectedInstance = instance;

            ODebugger.Debug($"Selected {instance} at {paintBox.MousePosition}");

            switch (instance)
            {
                case PathInstance pathInst:
                    active = Active.Path;
                    pathEdit = new(paintBox, pathInst);
                    pathEdit.Select();
                    break;
                case PointInstance pointInst:
                    active = Active.Point;
                    pointEdit = new(paintBox, pointInst);
                    pointEdit.Select();
                    break;
            }
        }
    }

    private (vec2 p, IList<Instance> inst, int i) _activeInsts = (vec2.Zero, Array.Empty<Instance>(), -1);
    private void SelectInstance12()
    {
        if (_activeInsts.i != -1)
        {
            if (vec2.Mag(paintBox.MousePosition, _activeInsts.p) <= 1)
            {
                SelectInstance(_activeInsts.inst[_activeInsts.i]);
                _activeInsts.i++;

                if (_activeInsts.i >= _activeInsts.inst.Count)
                    _activeInsts.i = 0;

                return;
            }
        }

        var instances = _Utils.SelectableInstances(Manager.Map!.Instances, paintBox.MousePosition, Manager.Settings.Select_PointTolerance).ToList();

        instances = instances.OrderBy(x => x.Item2).ToList();

        _activeInsts = (paintBox.MousePosition, instances.Select(x => x.Item1).ToList(), 1);

        SelectInstance(_activeInsts.inst[0]);
    }
    private void SelectInstance(Instance inst)
    {
        Deselect();

        selectedInstance = inst;

        ODebugger.Debug($"Selected {inst} at {paintBox.MousePosition}");

        switch (inst)
        {
            case PathInstance pathInst:
                active = Active.Path;
                pathEdit = new(paintBox, pathInst);
                pathEdit.Select();
                break;
            case PointInstance pointInst:
                active = Active.Point;
                pointEdit = new(paintBox, pointInst);
                pointEdit.Select();
                break;
        }
    }

    private void Deselect()
    {
        selectedInstance = null;

        pathEdit?.Deselect();
        pointEdit?.Deselect();

        pointEdit = null;
        pathEdit = null;

        active = Active.None;
    }


    public void Start() => IsActive = true;
    public void Stop() => IsActive = false;

    public static MapEdit Create() => new();
    
    private enum Active { None, Point, Path }
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
        DrawHelpers();
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

        WriteLine($"BBox: {bBox}");

        return new()
        {
            TopLeft = bBox.XY,
            Size = (bBox.ZW - bBox.XY).Abs(),

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
            TopLeft = _instance.Centre,
            Diameter = 2 * radius,

            BorderColour = Manager.Settings.Select_HandleAnchorColour,
            BorderWidth = Manager.Settings.Select_HandleLineWidth,

            ZIndex = Manager.Settings.Select_HandleZIndex,
        };
    }
}

public class PathEdit
{
    private PathInstance _instance;
    private PaintBox paintBox;
    private IMapRenderer2D _renderer;

    private Guid _helperId;

    private bool _active;

    public PathEdit(PaintBox paintBox, PathInstance instance)
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

    private (int, int, sbyte) _index;

    public void LMouseDown()
    {
        var (point, dist) = _Utils.NearestPoint(_instance.GetAllPoints(), paintBox.MousePosition);
        if (dist > Manager.Settings.Select_PointTolerance) return;

        _index = _Utils.FindPoint(_instance.Segments, point);
    }

    public void MovePoint()
    {
        if (_index.Item1 == -1)
        {
            ODebugger.Error("Couldn't Find Point");
            return;
        }

        switch (_index.Item3)
        {
            case -1: 
                LinearPath linear = (LinearPath)_instance.Segments[_index.Item1];

                if (_index.Item2 == 0 && _index.Item1 != 0)
                {
                    IPath prev = _instance.Segments[_index.Item1 - 1];
                    if (prev is LinearPath line && line[^1] == linear[_index.Item2])
                        line[^1] = paintBox.MousePosition;
                    else if (prev is BezierPath bez && bez[^1].Anchor == linear[_index.Item2])
                    {
                        BezierPoint bezier = bez[^1];
                        vec2 delta = paintBox.MousePosition - bezier.Anchor;

                        bezier = new()
                        {
                            Anchor = paintBox.MousePosition,
                            EarlyControl = bezier.EarlyControl.IsT0 ? (bezier.EarlyControl.AsT0 + delta) : new None(),
                            LateControl = bezier.LateControl.IsT0 ? (bezier.LateControl.AsT0 + delta) : new None(),
                        };

                        bez[^1] = bezier;
                    }
                }
                else if (_index.Item2 == linear.Count - 1 && _index.Item1 != _instance.Segments.Count - 1)
                {
                    IPath next = _instance.Segments[_index.Item1 + 1];
                    if (next is LinearPath line && line[0] == linear[_index.Item2])
                        line[0] = paintBox.MousePosition;
                    else if (next is BezierPath bez && bez[0].Anchor == linear[_index.Item2])
                    {
                        BezierPoint bezier = bez[0];
                        vec2 delta = paintBox.MousePosition - bezier.Anchor;

                        bezier = new()
                        {
                            Anchor = paintBox.MousePosition,
                            EarlyControl = bezier.EarlyControl.IsT0 ? (bezier.EarlyControl.AsT0 + delta) : new None(),
                            LateControl = bezier.LateControl.IsT0 ? (bezier.LateControl.AsT0 + delta) : new None(),
                        };

                        bez[0] = bezier;
                    }
                }

                linear[_index.Item2] = paintBox.MousePosition;
                _instance.Segments[_index.Item1] = linear;
                break;
            case 0: // Early Control
            {
                BezierPath path = (BezierPath)_instance.Segments[_index.Item1];
                BezierPoint bezier = path[_index.Item2];

                Assert(bezier.EarlyControl.IsT0, "Nearest Point is non-existent EarlyControl");

                vec2 normal = (bezier.Anchor - bezier.EarlyControl.AsT0).Normalise();
                bezier.EarlyControl = paintBox.MousePosition;

                if (bezier.LateControl.IsT0)
                {
                    vec2 late = bezier.LateControl.AsT0;
                    float mag = vec2.Mag(bezier.Anchor, late);

                    bezier.LateControl = bezier.Anchor + (normal * mag);
                }

                path[_index.Item2] = bezier;
                _instance.Segments[_index.Item1] = path;
            }
            break;
            case 1: // Anchor
            {
                BezierPath path = (BezierPath)_instance.Segments[_index.Item1];
                BezierPoint bezier = path[_index.Item2];

                vec2 delta = paintBox.MousePosition - bezier.Anchor;

                bezier = new()
                {
                    Anchor = paintBox.MousePosition,
                    EarlyControl = bezier.EarlyControl.IsT0 ? (bezier.EarlyControl.AsT0 + delta) : new None(),
                    LateControl = bezier.LateControl.IsT0 ? (bezier.LateControl.AsT0 + delta) : new None(),
                };

                if (_index.Item2 == 0 && _index.Item1 != 0)
                {
                    IPath prev = _instance.Segments[_index.Item1 - 1];
                    if (prev is LinearPath line && line[^1] == path[_index.Item2].Anchor)
                        line[^1] = paintBox.MousePosition;
                    else if (prev is BezierPath bez && bez[^1].Anchor == path[_index.Item2].Anchor)
                    {
                        BezierPoint prevBezier = bez[^1];
                        prevBezier.Anchor = paintBox.MousePosition;
                        bez[^1] = prevBezier;
                    }
                }
                else if (_index.Item2 == path.Count - 1 && _index.Item1 != _instance.Segments.Count - 1)
                {
                    IPath next = _instance.Segments[_index.Item1 + 1];
                    if (next is LinearPath line && line[0] == path[_index.Item2].Anchor)
                        line[0] = paintBox.MousePosition;
                    else if (next is BezierPath bez && bez[0].Anchor == path[_index.Item2].Anchor)
                    {
                        BezierPoint b = bez[0];
                        vec2 delta2 = paintBox.MousePosition - b.Anchor;

                        b = new()
                        {
                            Anchor = paintBox.MousePosition,
                            EarlyControl = b.EarlyControl.IsT0 ? (b.EarlyControl.AsT0 + delta2) : new None(),
                            LateControl = b.LateControl.IsT0 ? (b.LateControl.AsT0 + delta2) : new None(),
                        };

                        bez[0] = b;
                    }
                }

                path[_index.Item2] = bezier;
                _instance.Segments[_index.Item1] = path;
            } break;
            case 2: // Late Control
            {
                BezierPath path = (BezierPath)_instance.Segments[_index.Item1];
                BezierPoint bezier = path[_index.Item2];

                Assert(bezier.LateControl.IsT0, "Nearest Point is non-existent LateControl");

                vec2 normal = (bezier.Anchor - bezier.LateControl.AsT0).Normalise();
                bezier.LateControl = paintBox.MousePosition;

                if (bezier.EarlyControl.IsT0)
                {
                    vec2 early = bezier.EarlyControl.AsT0;
                    float mag = vec2.Mag(bezier.Anchor, early);

                    bezier.EarlyControl = bezier.Anchor + (normal * mag);
                }

                path[_index.Item2] = bezier;
                _instance.Segments[_index.Item1] = path;
            }
            break;
        }

        paintBox.Update(_instance.Id, _renderer.RenderInstance(_instance).ConvertCollection());
        DrawHelpers();
    }

    public void LMouseUp()
    {
        _index = (-1, -1, -2);
    }

    public void CtrlClick()
    {
        var (_, dist) = _Utils.NearestPoint(_instance.GetAllPoints(), paintBox.MousePosition);

        if (dist < Manager.Settings.Select_PointTolerance)
            RemovePoint();
        else
            InsertPoint();
    }

    public void InsertPoint()
    {
        var (segIndex, pointIndex, dist) = _Utils.NearestSegmentOnPathInstance(_instance, paintBox.MousePosition);

        if (dist < Manager.Settings.Select_PointTolerance)
        {
            if (segIndex == -1 || pointIndex == -1)
                return;

            switch (_instance.Segments[segIndex])
            {
                case LinearPath linear:
                    var list = linear.ToList();
                    list.Insert(pointIndex, paintBox.MousePosition);

                    _instance.Segments[segIndex] = new LinearPath(list);
                    break;
                case BezierPath bezier:
                    var cubics = bezier.AsCubicBezier();
                    CubicBezier bez = cubics[pointIndex];

                    var (nearest, _) = _Utils.NearestContinuousPointOnPathInstance(_instance, paintBox.MousePosition);
                    float t = _Utils.EstimateTValue(bez, nearest);

                    var (e, l) = BezierUtils.Subdivision(bez, t);

                    cubics[pointIndex] = e;
                    cubics.Insert(pointIndex + 1, l);

                    var newBez = BezierPath.FromCubicBeziers(cubics);
                    _instance.Segments[segIndex] = newBez;

                    break;
            }

            paintBox.Update(_instance.Id, _renderer.RenderInstance(_instance).ConvertCollection()); 
            DrawHelpers();
        }
    }

    public void RemovePoint()
    {
        var (point, _) = _Utils.NearestPoint(_instance.GetAllPoints(), paintBox.MousePosition);

        var _index = _Utils.FindPoint(_instance.Segments, point);

        if (_index.Item3 == -1)
            ((LinearPath)_instance.Segments[_index.Item1]).RemoveAt(_index.Item2);
        else if (_index.Item3 == 1)
            ((BezierPath)_instance.Segments[_index.Item1]).RemoveAt(_index.Item2);

        paintBox.Update(_instance.Id, _renderer.RenderInstance(_instance).ConvertCollection());
        DrawHelpers();
    }   

    public void Delete()
    {

    }

    private void DrawHelpers()
    {
        List<IShape> objs = new() { DrawBBox() };
        objs.AddRange(DrawHandles());

        paintBox.AddOrUpdate(_helperId, objs.ConvertCollection());
    }

    private Rectangle DrawBBox()
    {
        vec4 bBox = BoundingBox.OfInstance(_instance);
        
        return new()
        {
            TopLeft = bBox.XY - Manager.Settings.Select_BBoxOffset,
            Size = (bBox.ZW - bBox.XY).Abs() + (2 * Manager.Settings.Select_BBoxOffset),

            BorderColour = Manager.Settings.Select_BBoxColour,
            BorderWidth = Manager.Settings.Select_BBoxLineWidth,
            DashArray = Manager.Settings.Select_BBoxDashArray,

            ZIndex = Manager.Settings.Select_BBoxZIndex,
        };
    }

    private IEnumerable<IShape> DrawHandles()
    {
        var handles = _instance.GetAllPoints();

        foreach (vec2 v2 in handles)
        {
            yield return new Circle()
            {
                TopLeft = v2,
                Diameter = 2 * Manager.Settings.Select_HandleRadius,

                BorderColour = Manager.Settings.Select_HandleAnchorColour,
                BorderWidth = Manager.Settings.Select_HandleLineWidth,

                ZIndex = Manager.Settings.Select_HandleZIndex,
            };
        }

        var bezs = _instance.GetAllBeziers();

        foreach (var bez in bezs)
        {
            if (bez.EarlyControl.IsT0)
            {
                vec4 line = (bez.EarlyControl.AsT0, bez.Anchor);
                line = _Utils.RemoveCircleFromLine(line, Manager.Settings.Select_HandleRadius);

                yield return new Line()
                {
                    Points = new() { line.XY, line.ZW },

                    Colour = Manager.Settings.Select_HandleAnchorColour,
                    Width = Manager.Settings.Select_HandleLineWidth,

                    ZIndex = Manager.Settings.Select_HandleZIndex,
                    Opacity = 1f,
                };
            }
            if (bez.LateControl.IsT0)
            {
                vec4 line = (bez.LateControl.AsT0, bez.Anchor);
                line = _Utils.RemoveCircleFromLine(line, Manager.Settings.Select_HandleRadius);

                yield return new Line()
                {
                    Points = new() { line.XY, line.ZW },

                    Colour = Manager.Settings.Select_HandleAnchorColour,
                    Width = Manager.Settings.Select_HandleLineWidth,

                    ZIndex = Manager.Settings.Select_HandleZIndex,
                    Opacity = 1f,
                };
            }
        }
    }
}

internal static class _Utils
{
    public static (Instance?, float) NearestInstance(IEnumerable<Instance> coll, vec2 pos)
    {
        if (!coll.Any()) return (null, float.MaxValue);

        ReadOnlySpan<Instance> collection = coll.ToArray();

        Instance closest = collection[0];
        float closestDist = float.MaxValue;

        foreach (Instance instance in collection)
        {
            switch (instance)
            {
                case PointInstance point:
                {
                    float mag = vec2.Mag(point.Centre, pos);

                    float extrusion = _Utils.ObjectExtrusion(point.Symbol);

                    if (mag - extrusion < closestDist)
                    {
                        closest = instance;
                        closestDist = mag - extrusion;
                    }
                } break;

                case LineInstance path:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(path, pos);

                    if (dist < closestDist)
                    {
                        closest = instance;
                        closestDist = dist;
                    }
                } break;

                case AreaInstance area:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(area, pos);

                    if (dist < closestDist)
                    {
                        closest = instance;
                        closestDist = dist;
                    }
                } break;
            }
        }

        return (closest, closestDist);
    }

    public static IEnumerable<(Instance, float)> SelectableInstances(IEnumerable<Instance> coll, vec2 pos, float radius)
    {
        ReadOnlySpan<Instance> collection = coll.ToArray();
        if (collection.IsEmpty) return Enumerable.Empty<(Instance, float)>();

        List<(Instance, float)> instances = new();

        foreach (Instance inst in collection)
        {
            switch (inst)
            {
                case PointInstance point:
                {
                    float mag = vec2.Mag(point.Centre, pos);
                    float extrusion = _Utils.ObjectExtrusion(point.Symbol);

                    if ((mag - extrusion) <= radius)
                        instances.Add((inst, mag - extrusion));
                } break;
                case LineInstance line:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(line, pos);

                    if (dist <= radius)
                        instances.Add((inst, dist));
                } break;
                case AreaInstance area:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(area, pos);

                    if (dist <= radius)
                        instances.Add((inst, dist));

                    if (PolygonTools.IsPointInPoly(PolygonTools.ToPolygon(area), Enumerable.Empty<IList<vec2>>(), pos))
                        instances.Add((inst, 0));

                } break;
            }
        }

        return instances;
    }

    public static float ObjectExtrusion(PointSymbol sym)
    {
        float dist = float.MinValue;

        foreach (var obj in sym.MapObjects)
        {
            switch (obj)
            {
                case PointObject pObj:
                {
                    if (pObj.InnerRadius + pObj.OuterRadius > dist)
                        dist = pObj.InnerRadius + pObj.OuterRadius;
                } break;
                case LineObject lObj:
                {
                    foreach (vec2 p in lObj.Segments.LinearApproximation())
                    {
                        float mag = p.Mag();

                        if (mag > dist)
                            dist = mag;
                    }
                } break;
                case AreaObject aObj:
                {
                    foreach (vec2 p in aObj.Segments.LinearApproximation())
                    {
                        float mag = p.Mag();

                        if (mag > dist)
                            dist = mag;
                    }
                } break;
                case TextObject tObj: break;
            }
        }

        return dist;
    }

    public static (vec2, float) NearestContinuousPointOnPathInstance(PathInstance inst, vec2 pos)
    {
        vec2 nearest = vec2.Zero;
        float dist = float.MaxValue;

        foreach (IPath seg in inst.Segments)
        {
            switch (seg)
            {
                case LinearPath line:
                {
                    for (int i = 1; i < line.Count(); i++)
                    {
                        vec2 nearestPointOnSegment = NearestPointOnLineSegment(line[i - 1], line[i], pos);
                        float distPToP = vec2.Mag(nearestPointOnSegment, pos);

                        if (distPToP < dist)
                        {
                            nearest = nearestPointOnSegment;
                            dist = distPToP;
                        }
                    }
                } break;
                case BezierPath bez: // Not sure if most efficient
                {
                    var linearised = bez.LinearApproximation();

                    for (int i = 1; i < linearised.Count; i++)
                    {
                        vec2 nearestPointOnSegment = NearestPointOnLineSegment(linearised[i - 1], linearised[i], pos);
                        float distPToP = vec2.Mag(nearestPointOnSegment, pos);

                        if (distPToP < dist)
                        {
                            nearest = nearestPointOnSegment;
                            dist = distPToP;
                        }
                    }
                } break;
            }
        }

        return (nearest, dist);
    }

    public static vec2 NearestPointOnLineSegment(vec2 a, vec2 b, vec2 pos)
    {
        float gradAB = (b.Y - a.Y) / (b.X - a.X);
        float perpGrad = -1 / gradAB;

        if (gradAB == 0)
            return new(pos.X, a.Y);

        if (float.IsInfinity(gradAB))
            return new(a.X, pos.Y);

        float perpC = pos.Y - (perpGrad * pos.X);
        float abC = a.Y - (gradAB * a.X);

        float x = (-perpC + abC) / (-gradAB + perpGrad);
        float y = ((abC * perpGrad) - (perpC * gradAB)) / (-gradAB + perpGrad);

        vec2 output = new(x, y);

        if (vec2.Mag(output, a) > vec2.Mag(b, a))
            return a;

        if (vec2.Mag(output, b) > vec2.Mag(a, b))
            return b;

        return output;
    }
    
    public static vec2 NearestPointOnLine(IList<vec2> line, vec2 pos)
    {
        vec2 nearest = vec2.Zero;
        float dist = float.MaxValue;

        for (int i = 1; i < line.Count; i++)
        {
            vec2 p = NearestPointOnLineSegment(line[i - 1], line[i], pos);
            float d = vec2.Mag(p, pos);

            if (d < dist)
            {
                nearest = p;
                dist = d;
            }
        }

        return nearest;
    }

    public static (vec2, float) NearestPoint(IEnumerable<vec2> points, vec2 p)
    {
        vec2 nearest = vec2.Zero;
        float dist = float.MaxValue;

        foreach (vec2 point in points)
        {
            float mag = vec2.Mag(point, p);

            if (mag < dist)
            {
                nearest = point;
                dist = mag;
            }
        }

        return (nearest, dist);
    }

    public static IEnumerable<vec2> GetAllPoints(this PathInstance inst)
    {
        List<vec2> points = new(inst.Segments.GetPoints());

        foreach (var hole in inst.Holes)
            points.AddRange(hole.GetPoints());

        return points;
    }
    public static IEnumerable<BezierPoint> GetAllBeziers(this PathInstance inst)
    {
        List<BezierPoint> points = new(inst.Segments.GetBeziers());

        foreach (var hole in inst.Holes)
            points.AddRange(hole.GetBeziers());

        return points;
    }
    
    public static (int, int, sbyte) FindPoint(PathCollection pC, vec2 v2)
    {
        for (int i = 0; i < pC.Count; i++)
        {
            switch (pC[i])
            {
                case LinearPath line:
                {
                    if (line.Contains(v2))
                        return (i, line.IndexOf(v2), -1);
                } break;
                case BezierPath bez:
                {
                    foreach (var b in bez)
                    {
                        if (b.EarlyControl.IsT0 && b.EarlyControl.AsT0 == v2)
                            return (i, bez.IndexOf(b), 0);
                     
                        if (b.Anchor == v2)
                            return (i, bez.IndexOf(b), 1);

                        if (b.LateControl.IsT0 && b.LateControl.AsT0 == v2)
                            return (i, bez.IndexOf(b), 2);
                    }
                } break;
            }
        }

        return (-1, -1, -2);
    }

    public static (int, int, float) NearestSegmentOnPathInstance(PathInstance inst, vec2 v2)
    {
        int indexA = -1, indexB = -1;
        float dist = float.MaxValue;

        for (int i = 0; i < inst.Segments.Count; i++)
        {
            switch (inst.Segments[i])
            {
                case LinearPath path:
                {
                    for (int j = 1; j < path.Count; j++)
                    {
                        vec2 p = NearestPointOnLineSegment(path[j - 1], path[j], v2);
                        float d = vec2.Mag(p, v2);

                        if (d < dist)
                        {
                            indexA = i;
                            indexB = j;
                            dist = d;
                        }
                    }
                } break;
                case BezierPath bez:
                {
                    var cubics = bez.AsCubicBezier();

                    for (int j = 0; j < cubics.Count; j++)
                    {
                        vec2 n = NearestPointOnLine(cubics[j].LinearApproximation(), v2);
                        float d = vec2.Mag(n, v2);

                        if (d < dist)
                        {
                            indexA = i;
                            indexB = j;
                            dist = d;
                        }
                    }
                } break;
            }
        }

        return (indexA, indexB, dist);
    }

    public static vec4 RemoveCircleFromLine(vec4 line, float radius1, float radius2 = -1f)
    {
        float length = vec2.Mag(line.XY, line.ZW);
        float ratio = radius1 / length;

        vec2 xy = vec2.Lerp(line.XY, line.ZW, ratio);

        vec2 zw = (radius2 == -1f)
            ? vec2.Lerp(line.ZW, line.XY, ratio)
            : vec2.Lerp(line.ZW, line.XY, radius2 / length);

        return new(xy, zw);
    }

    public static float EstimateTValue(CubicBezier bez, vec2 v2)
    {
        const float resolution = 1f / 200;
        float error = Manager.Settings.Select_PointTolerance;

        for (float t = 0; t <= 1; t += resolution)
        {
            vec2 p = BezierUtils.Lerp(bez, t);

            if (vec2.Mag(p, v2) < error)
                return t;

        }

        throw new Exception("No t value found");
    }

    public static bool IsInRect(vec4 rect, vec2 p)
    {
        return p.X >= rect.X && p.X <= rect.Z && p.Y >= rect.Y && p.Y <= rect.W;
    }
}