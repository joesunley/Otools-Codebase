using OTools.Courses;
using OTools.Maps;

namespace OTools.ObjectRenderer2D;

public class SimpleCourseRenderer2D : IVisualRenderer
{
    private readonly Event _activeEvent;
    private readonly IMapRenderer2D _symbolRenderer;

    public SimpleCourseRenderer2D(Event activeEvent)
    {
        _activeEvent = activeEvent;
        _symbolRenderer = new MapRenderer2D(null);
    }

    public IEnumerable<(Guid, IEnumerable<IShape>)> Render()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IShape> RenderControl(Control control)
    {
        PointSymbol sym = _Utils.GetSymbol(control.Type, _activeEvent.Symbols);

        PointInstance inst = new(0, sym, control.Position, control.Rotation);

        return _symbolRenderer.RenderPointInstance(inst);
    }

    public IEnumerable<IShape> RenderCourseLines(LinearCourse course)
    {
        List<vec4> lines = new();

        if (course.Parts is LinearCoursePart part)
        {
            for (int i = 1; i < part.Count; i++)
            {
                vec2 c1 = part[i - 1].Position,
                    c2 = part[i].Position;

                vec4 l = (c1, c2);

                lines.Add(_Utils.RemoveCircleFromLine(l,
                   _Utils.ObjectExtrusion(_Utils.GetSymbol(part[i-1].Type, _activeEvent.Symbols)),
                   _Utils.ObjectExtrusion(_Utils.GetSymbol(part[i].Type, _activeEvent.Symbols))));
            }
        }

        List<IShape> shapes = new();

        foreach (vec4 line in lines)
        {

            vec2[] ps = { line.XY, line.ZW };
            PathCollection pc = new(ps);

            LineInstance inst = new(0, _activeEvent.Symbols.CourseLine, pc, false);
            shapes.AddRange(_symbolRenderer.RenderPathInstance(inst));
        }

        return shapes;
    }
}

public class CourseRenderer2D : IVisualRenderer
{
    private readonly Event _activeEvent;
    private readonly IMapRenderer2D _symbolRenderer;

    public CourseRenderer2D(Event activeEvent)
    {
        _activeEvent = activeEvent;
        _symbolRenderer = new MapRenderer2D(null);
    }

    public IEnumerable<(Guid, IEnumerable<IShape>)> Render()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IShape> RenderNormalControl(Control control)
    {
        var shapes = _symbolRenderer.RenderPointSymbol(_activeEvent.Symbols.Control);

        return shapes.Select(x => { x.TopLeft = control.Position; return x; });
    }

    public IEnumerable<IShape> RenderLinearCourse(LinearCourse course)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IShape> RenderItems(ItemStore items, Course activeCourse)
    {
        List<IShape> shapes = new();

        foreach (var item in items)
        {
            if (item.VisibleCourses.Contains(activeCourse.Id))
                shapes.AddRange(RenderItem(item));
        }

        return shapes;
    }
    public IEnumerable<IShape> RenderItem(Item item)
    {
        throw new NotImplementedException();
    }
}

internal static partial class _Utils
{
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

    public static PointSymbol GetSymbol(ControlType type, CourseSymbols syms)
    {
        return type switch
        {
            ControlType.Normal => syms.Control,
            ControlType.Start => syms.Start,
            ControlType.Finish => syms.Finish,
            ControlType.Exchange => syms.Exchange,
            ControlType.CrossingPoint => syms.CrossingPoint,
            _ => throw new InvalidOperationException(),
        };
    }
}
