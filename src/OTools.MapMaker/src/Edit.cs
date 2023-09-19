using Avalonia.Controls.Utils;
using Avalonia.Input;
using OneOf.Types;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using ownsmtp.logging;
using Svg.Model.Drawables.Elements;
using System.Diagnostics.Contracts;
using System.Dynamic;

namespace OTools.MapMaker;

public class MapEdit
{
    private MapMakerInstance _instance;

    public MapEdit(MapMakerInstance instance)
    {
        _instance = instance;

        _instance.ActiveToolChanged += args =>
        {
            IsActive = (args == Tool.Edit);

            if (!IsActive && _selectedInstance != null)
            {
                Deselect();
            }
        };

        _instance.PaintBox.PointerPressed += (_, args) => MouseDown(args);
        _instance.PaintBox.PointerReleased += (_, args) => MouseUp(args.InitialPressMouseButton, args.KeyModifiers);
        _instance.PaintBox.PointerMoved += (_, args) => MouseMove(args);
        _instance.PaintBox.KeyUp += (_, args) => KeyUp(args.Key);
        _instance.PaintBox.ZoomChanged += (_) => Zoom();
    }

    private PointEdit? _pointEdit;
    private PathEdit? _pathEdit;

    public bool IsActive { get; set; }

    private Instance? _selectedInstance;

    private void MouseDown(PointerPressedEventArgs args)
    {
        if (!IsActive) return;

        if (args.GetCurrentPoint(_instance.PaintBox.canvas).Properties.IsLeftButtonPressed)
            _pathEdit?.LeftMouseDown();
    }

    private void MouseUp(MouseButton mouse, KeyModifiers modifiers)
    {
        if (!IsActive) return;

        switch (mouse)
        {
            case MouseButton.Left:
                if (_selectedInstance is null)
                    Select();
                if (modifiers.HasFlag(KeyModifiers.Control))
                    _pathEdit?.CtrlClick();

                _pathEdit?.LeftMouseUp();
                break;
        }
    }

    private void MouseMove(MouseMovedEventArgs args)
    {
        if (!IsActive) return;

        if (args.Properties.IsLeftButtonPressed)
        {
            _pointEdit?.Move();
            _pathEdit?.MovePoint();
        }
    }

    private void KeyUp(Key key)
    {
        if (!IsActive) return;

        switch (key)
        {
            case Key.Delete:
                _pointEdit?.Delete();
                _pathEdit?.Delete();
                break;
            case Key.Escape:
                Deselect();
                break;
        }
    }

    private void Zoom()
    {
        _pointEdit?.DrawHelpers();
        _pathEdit?.DrawHelpers();
    }

    private void Select()
    {
        var (inst, dist) = _Utils.NearestInstance(_instance.Map.Instances, _instance.PaintBox.MousePosition);

        if (inst is null || dist > _instance.Settings.Select_ObjectTolerance)
            return;

        Deselect();

        _selectedInstance = inst;
        ODebugger.Debug($"Selected: {_selectedInstance.Id} at {_instance.PaintBox.MousePosition}");

        if (inst is PathInstance path)
        {
            _pointEdit = null;
            _pathEdit = new(_instance, path);

            _pathEdit.Select();
        }
        else if (inst is PointInstance point)
        {
            _pointEdit = new(_instance, point);
            _pathEdit = null;

            _pointEdit.Select();
        }
    }
    private void Select_Range()
    {

    }
    private void Select(Instance inst)
    {

    }
    private void Deselect()
    {
        _selectedInstance = null;

        _pointEdit?.Deselect();
        _pathEdit?.DeSelect();

        _pointEdit = null;
        _pathEdit = null;
    }

    public void Start() => IsActive = true;
    public void Stop() => IsActive = false;
}

public class PointEdit
{
    private MapMakerInstance _mInstance;
    private PointInstance _pInstance;

    private Guid _helperId;
    private bool _active;

    public PointEdit(MapMakerInstance mInstance, PointInstance pInstance)
    {
        _mInstance = mInstance;
        _pInstance = pInstance;

        _helperId = Guid.NewGuid();
        _active = false;
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

        _mInstance.PaintBox.Remove(_helperId);
    }

    public void Move()
    {
        _pInstance.Centre = _mInstance.PaintBox.MousePosition;

        _mInstance.PaintBox.Update(_pInstance.Id,
            _mInstance.MapRenderer.RenderPointInstance(_pInstance).ConvertCollection());

        DrawHelpers();
    }

    public void Delete()
    {
        _mInstance.PaintBox.Remove(_pInstance.Id);
        _mInstance.PaintBox.Remove(_helperId);

        _mInstance.Map.Instances.Remove(_pInstance);
    }

    public void DrawHelpers()
    {
        vec4 bBOx = BoundingBox.OfInstance(_pInstance);

        IShape[] objs =
        {
            new Circle()
            {
                TopLeft = _pInstance.Centre,
                Diameter = 2 * _mInstance.Settings.Select_HandleRadius * (1 / _mInstance.PaintBox.Zoom.X),

                BorderColour = _mInstance.Settings.Select_HandleAnchorColour,
                BorderWidth = _mInstance.Settings.Select_HandleLineWidth * (1 / _mInstance.PaintBox.Zoom.X),

                ZIndex = _mInstance.Settings.Select_HandleZIndex,
            },

            new Rectangle()
            {
                TopLeft = bBOx.XY,
                Size = (bBOx.ZW - bBOx.XY).Abs(),

                BorderColour = _mInstance.Settings.Select_BBoxColour,
                BorderWidth = _mInstance.Settings.Select_BBoxLineWidth *(1 / _mInstance.PaintBox.Zoom.X),
                DashArray = _mInstance.Settings.Select_BBoxDashArray.Select(x => x * (1 / _mInstance.PaintBox.Zoom.X)),

                ZIndex = _mInstance.Settings.Select_BBoxZIndex,
            }
        };

        _mInstance.PaintBox.AddOrUpdate(_helperId, objs.ConvertCollection());
    }
}

public class PathEdit
{
    private MapMakerInstance _mInstance;
    private PathInstance _pInstance;

    private Guid _helperId;
    private bool _active;

    public PathEdit(MapMakerInstance mInstance, PathInstance pInstance)
    {
        _mInstance = mInstance;
        _pInstance = pInstance;

        _helperId = Guid.NewGuid();
        _active = false;
    }

    public void Select()
    {
        if (_active) return;
        _active = true;

        DrawHelpers();
    }
    public void DeSelect()
    {
        if (!_active) return;
        _active = false;

        _mInstance.PaintBox.Remove(_helperId);
    }

    private (int, int, sbyte) _index;

    public void LeftMouseDown()
    {
        var (point, dist) = _Utils.NearestPoint(_pInstance.GetAllPoints(), _mInstance.PaintBox.MousePosition);

        if (dist > _mInstance.Settings.Select_PointTolerance)
            return;

        _index = _Utils.FindPoint(_pInstance.Segments, point);

        if (_index.Item1 == -1)
            ODebugger.Error($"Couldn't find point: {point}");
    }
    public void LeftMouseUp()
    {
        _index = (-1, -1, -2);
    }

    public void MovePoint()
    {
        if (_index.Item1 == -1) return;

        switch (_index.Item3)
        {
            case -1:
            {
                LinearPath linear = (LinearPath)_pInstance.Segments[_index.Item1];

                if (_index.Item2 == 0 && _index.Item1 != 0)
                {
                    IPath prev = _pInstance.Segments[_index.Item1 - 1];

                    if (prev is LinearPath line && line[^1] == linear[_index.Item2])
                        line[^1] = _mInstance.PaintBox.MousePosition;

                    else if (prev is BezierPath bez && bez[^1].Anchor == linear[_index.Item2])
                    {
                        BezierPoint bezier = bez[^1];
                        vec2 delta = _mInstance.PaintBox.MousePosition - bezier.Anchor;

                        bezier = new()
                        {
                            Anchor = _mInstance.PaintBox.MousePosition,
                            EarlyControl = bezier.EarlyControl.IsT0 ?
                                bezier.EarlyControl.AsT0 + delta :
                                new None(),
                            LateControl = bezier.LateControl.IsT0 ?
                                bezier.LateControl.AsT0 + delta :
                                new None(),
                        };

                        bez[^1] = bezier;
                    }
                }
                else if (_index.Item2 == linear.Count - 1 && _index.Item1 != _pInstance.Segments.Count - 1)
                {
                    IPath next = _pInstance.Segments[_index.Item1 + 1];

                    if (next is LinearPath line && line[0] == linear[_index.Item2])
                        line[0] = _mInstance.PaintBox.MousePosition;

                    else if (next is BezierPath bez && bez[0].Anchor == linear[_index.Item2])
                    {
                        BezierPoint bezier = bez[0];
                        vec2 delta = _mInstance.PaintBox.MousePosition - bezier.Anchor;

                        bezier = new()
                        {
                            Anchor = _mInstance.PaintBox.MousePosition,
                            EarlyControl = bezier.EarlyControl.IsT0 ?
                                bezier.EarlyControl.AsT0 + delta :
                                new None(),
                            LateControl = bezier.LateControl.IsT0 ?
                                bezier.LateControl.AsT0 + delta :
                                new None(),
                        };

                        bez[0] = bezier;
                    }
                }

                linear[_index.Item2] = _mInstance.PaintBox.MousePosition;
                _pInstance.Segments[_index.Item1] = linear;
            } break;

            case 0: // Early Control
            {
                BezierPath path = (BezierPath)_pInstance.Segments[_index.Item1];
                BezierPoint bezier = path[_index.Item2];

                ODebugger.Assert(bezier.EarlyControl.IsT0, "Nearest Point is non-existent EarlyControl");

                bezier.EarlyControl = _mInstance.PaintBox.MousePosition;

                vec2 normal = (bezier.Anchor - bezier.EarlyControl.AsT0).Normalise();
                if (bezier.LateControl.IsT0)
                {
                    vec2 late = bezier.LateControl.AsT0;
                    float mag = vec2.Mag(bezier.Anchor, late);

                    bezier.LateControl = bezier.Anchor + (normal * mag);
                }

                path[_index.Item2] = bezier;
                _pInstance.Segments[_index.Item1] = path;

            } break;
            case 1: // Anchor
            {
                BezierPath path = (BezierPath)_pInstance.Segments[_index.Item1];
                BezierPoint bezier = path[_index.Item2];

                vec2 delta = _mInstance.PaintBox.MousePosition - bezier.Anchor;

                bezier = new()
                {
                    Anchor = _mInstance.PaintBox.MousePosition,
                    EarlyControl = bezier.EarlyControl.IsT0 ?
                        bezier.EarlyControl.AsT0 + delta :
                        new None(),
                    LateControl = bezier.LateControl.IsT0 ?
                        bezier.LateControl.AsT0 + delta :
                        new None(),
                };

                path[_index.Item2] = bezier;

                if (_index.Item2 == 0 && _index.Item1 != 0)
                {
                    IPath prev = _pInstance.Segments[_index.Item1 - 1];

                    if (prev is LinearPath line && line[^1] == path[_index.Item2].Anchor)
                        line[^1] = _mInstance.PaintBox.MousePosition;

                    else if (prev is BezierPath bez && bez[^1].Anchor == path[_index.Item2].Anchor)
                    {
                        BezierPoint prevBez = bez[^1];
                        prevBez.Anchor = _mInstance.PaintBox.MousePosition;
                        bez[^1] = prevBez;
                    }
                }
                else if (_index.Item2 == path.Count - 1 && _index.Item1 != _pInstance.Segments.Count - 1)
                {
                    IPath next = _pInstance.Segments[_index.Item1 + 1];

                    if (next is LinearPath line && line[0] == path[_index.Item2].Anchor)
                        line[0] = _mInstance.PaintBox.MousePosition;

                    else if (next is BezierPath bez && bez[0].Anchor == path[_index.Item2].Anchor)
                    {
                        BezierPoint nextBez = bez[0];
                        vec2 delta2 = _mInstance.PaintBox.MousePosition - nextBez.Anchor;

                        nextBez = new()
                        {
                            Anchor = _mInstance.PaintBox.MousePosition,
                            EarlyControl = nextBez.EarlyControl.IsT0 ?
                                nextBez.EarlyControl.AsT0 + delta2 :
                                new None(),
                            LateControl = nextBez.LateControl.IsT0 ?
                                nextBez.LateControl.AsT0 + delta2 :
                                new None(),
                        };

                        bez[0] = nextBez;
                    }
                }

                _pInstance.Segments[_index.Item1] = path;
            } break;
            case 2: // Late Control
            {
                BezierPath path = (BezierPath)_pInstance.Segments[_index.Item1];
                BezierPoint bezier = path[_index.Item2];

                ODebugger.Assert(bezier.LateControl.IsT0, "Nearest Point is non-existent LateControl");

                bezier.LateControl = _mInstance.PaintBox.MousePosition;

                vec2 normal = (bezier.Anchor - bezier.LateControl.AsT0).Normalise();

                if (bezier.EarlyControl.IsT0)
                {
                    vec2 early = bezier.EarlyControl.AsT0;
                    float mag = vec2.Mag(bezier.Anchor, early);

                    bezier.EarlyControl = bezier.Anchor + (normal * mag);
                }

                path[_index.Item2] = bezier;
                _pInstance.Segments[_index.Item1] = path;
            }
            break;
        }

        _mInstance.PaintBox.Update(_pInstance.Id,
            _mInstance.MapRenderer.RenderPathInstance(_pInstance).ConvertCollection());

        DrawHelpers();
    }

    public void CtrlClick()
    {
        var (_, dist) = _Utils.NearestPoint(_pInstance.GetAllPoints(), _mInstance.PaintBox.MousePosition);

        if (dist < _mInstance.Settings.Select_PointTolerance)
            RemovePoint();
        else
            InsertPoint();
    }

    private void InsertPoint()
    {
        var (segIndex, pointIndex, dist) = _Utils.NearestSegmentOnPathInstance(_pInstance, _mInstance.PaintBox.MousePosition);

        if (dist > _mInstance.Settings.Select_PointTolerance || segIndex == -1 || pointIndex == -1)
            return;

        switch (_pInstance.Segments[segIndex])
        {
            case LinearPath linear:
            {
                var points = linear.ToList();
                points.Insert(pointIndex, _mInstance.PaintBox.MousePosition);

                _pInstance.Segments[segIndex] = new LinearPath(points);
            } break;
            case BezierPath bezier:
            {
                var cubics = bezier.AsCubicBezier();
                CubicBezier bez = cubics[pointIndex];

                var (nearest, _) = _Utils.NearestContinuousPointOnPathInstance(_pInstance, _mInstance.PaintBox.MousePosition);
                float t = _Utils.EstimateTValue(bez, nearest, _mInstance.Settings.Select_PointTolerance);

                var (e, l) = BezierUtils.Subdivision(bez, t);

                cubics[pointIndex] = e;
                cubics.Insert(pointIndex + 1, l);

                var newBez = BezierPath.FromCubicBeziers(cubics);
                _pInstance.Segments[segIndex] = newBez;
            } break;
        }

        _mInstance.PaintBox.Update(_pInstance.Id,
            _mInstance.MapRenderer.RenderPathInstance(_pInstance).ConvertCollection());

        DrawHelpers();
    }

    private void RemovePoint()
    {
        var (point, _) = _Utils.NearestPoint(_pInstance.GetAllPoints(), _mInstance.PaintBox.MousePosition);

        var index = _Utils.FindPoint(_pInstance.Segments, point);

        if (index.Item3 == -1)
            ((LinearPath)_pInstance.Segments[_index.Item1]).RemoveAt(_index.Item2);
        else if (index.Item3 == 1)
            ((BezierPath)_pInstance.Segments[_index.Item1]).RemoveAt(_index.Item2);

        _mInstance.PaintBox.Update(_pInstance.Id,
            _mInstance.MapRenderer.RenderPathInstance(_pInstance).ConvertCollection());

        DrawHelpers();
    }

    public void Delete()
    {

    }

    public void DrawHelpers()
    {
        vec4 bBox = BoundingBox.OfInstance(_pInstance);
        var handles = _pInstance.GetAllPoints();
        var bezs = _pInstance.GetAllBeziers();

        List<IShape> objs = new()
        {
            new Rectangle()
            {
                TopLeft = bBox.XY - _mInstance.Settings.Select_BBoxOffset,
                Size = (bBox.ZW - bBox.XY).Abs() + (2 * _mInstance.Settings.Select_BBoxOffset),

                BorderColour = _mInstance.Settings.Select_BBoxColour,
                BorderWidth = _mInstance.Settings.Select_BBoxLineWidth * (1 / _mInstance.PaintBox.Zoom.X),
                DashArray = _mInstance.Settings.Select_BBoxDashArray,

                ZIndex = _mInstance.Settings.Select_BBoxZIndex,
            }
        };

        foreach (vec2 handle in handles)
        {
            objs.Add(new Circle()
            {
                TopLeft = handle,
                Diameter = 2 * _mInstance.Settings.Select_HandleRadius * (1 / _mInstance.PaintBox.Zoom.X),

                BorderColour = _mInstance.Settings.Select_HandleAnchorColour,
                BorderWidth = _mInstance.Settings.Select_HandleLineWidth * (1 / _mInstance.PaintBox.Zoom.X),

                ZIndex = _mInstance.Settings.Select_HandleZIndex,
            });
        }

        foreach (var bez in bezs)
        {
            if (bez.EarlyControl.IsT0)
            {
                vec4 line = (bez.EarlyControl.AsT0, bez.Anchor);
                line = _Utils.RemoveCircleFromLine(line, _mInstance.Settings.Select_HandleRadius * (1 / _mInstance.PaintBox.Zoom.X));

                objs.Add(new Line()
                {
                    Points = new() { line.XY, line.ZW },

                    Colour = _mInstance.Settings.Select_HandleControlColour,
                    Width = _mInstance.Settings.Select_HandleLineWidth * (1 / _mInstance.PaintBox.Zoom.X),

                    Opacity = 1f,
                    ZIndex = _mInstance.Settings.Select_HandleZIndex,
                });
            }

            if (bez.LateControl.IsT0)
            {
                vec4 line = (bez.LateControl.AsT0, bez.Anchor);
                line = _Utils.RemoveCircleFromLine(line, _mInstance.Settings.Select_HandleRadius * (1 / _mInstance.PaintBox.Zoom.X));

                objs.Add(new Line()
                {
                    Points = new() { line.XY, line.ZW },

                    Colour = _mInstance.Settings.Select_HandleControlColour,
                    Width = _mInstance.Settings.Select_HandleLineWidth * (1 / _mInstance.PaintBox.Zoom.X),

                    Opacity = 1f,
                    ZIndex = _mInstance.Settings.Select_HandleZIndex,
                });
            }
        }

        _mInstance.PaintBox.AddOrUpdate(_helperId, objs.ConvertCollection());
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
                }
                break;

                case LineInstance path:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(path, pos);

                    if (dist < closestDist)
                    {
                        closest = instance;
                        closestDist = dist;
                    }
                }
                break;

                case AreaInstance area:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(area, pos);

                    if (dist < closestDist)
                    {
                        closest = instance;
                        closestDist = dist;
                    }
                }
                break;
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
                }
                break;
                case LineInstance line:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(line, pos);

                    if (dist <= radius)
                        instances.Add((inst, dist));
                }
                break;
                case AreaInstance area:
                {
                    var (_, dist) = _Utils.NearestContinuousPointOnPathInstance(area, pos);

                    if (dist <= radius)
                        instances.Add((inst, dist));

                    if (PolygonTools.IsPointInPoly(PolygonTools.ToPolygon(area), Enumerable.Empty<IList<vec2>>(), pos))
                        instances.Add((inst, 0));

                }
                break;
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
                }
                break;
                case LineObject lObj:
                {
                    foreach (vec2 p in lObj.Segments.LinearApproximation())
                    {
                        float mag = p.Mag();

                        if (mag > dist)
                            dist = mag;
                    }
                }
                break;
                case AreaObject aObj:
                {
                    foreach (vec2 p in aObj.Segments.LinearApproximation())
                    {
                        float mag = p.Mag();

                        if (mag > dist)
                            dist = mag;
                    }
                }
                break;
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
                }
                break;
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
                }
                break;
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
                }
                break;
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
                }
                break;
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
                }
                break;
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
                }
                break;
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

    public static float EstimateTValue(CubicBezier bez, vec2 v2, float pointTolerance)
    {
        const float resolution = 1f / 200;
        float error = pointTolerance;

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