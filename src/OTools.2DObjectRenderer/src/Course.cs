using OTools.Courses;
using OTools.Maps;
using ownsmtp.logging;
using Sunley.Mathematics;
using TerraFX.Interop.Windows;

namespace OTools.ObjectRenderer2D;

public interface ICourseRenderer2D : IVisualRenderer
{
    IEnumerable<IShape> RenderControls(IEnumerable<Control> controls);
    IEnumerable<IShape> RenderControl(Control control);
    IEnumerable<IShape> RenderNormalControl(Control control);
    IEnumerable<IShape> RenderStartControl(Control control);
    IEnumerable<IShape> RenderFinishControl(Control control);
    IEnumerable<IShape> RenderCrossingPointControl(Control control);

    IEnumerable<IShape> RenderControlNumber(string text, vec2 controlPosition);

    IEnumerable<IShape> RenderCourse(Course course);
    IEnumerable<IShape> RenderLinearCourse(LinearCourse course);
    IEnumerable<IShape> RenderScoreCourse(ScoreCourse course);

    IEnumerable<IShape> RenderCoursePart(ICoursePart part, string variation = "");
    IEnumerable<IShape> RenderLinearCoursePart(LinearCoursePart part, string variation = "");
    IEnumerable<IShape> RenderCombinedCoursePart(CombinedCoursePart part, string variation = "");
    IEnumerable<IShape> RenderVariationCoursePart(VariationCoursePart part, string variation = "");
    IEnumerable<IShape> RenderButterflyCoursePart(ButterflyCoursePart part, string variation = "");
    IEnumerable<IShape> RenderPhiLoopCoursePart(PhiLoopCoursePart part, string variation = "");
}

public class CourseRenderer2D : ICourseRenderer2D
{
    private readonly Event _activeEvent;
    private readonly Dictionary<int, IEnumerable<IShape>> _symbolCache;

    private readonly MapRenderer2D _mapRenderer;

    public CourseRenderer2D(Event ev)
    {
        _activeEvent = ev;
        _symbolCache = new();
        _mapRenderer = new(_activeEvent.SymbolMap 
            ?? throw new Exception("Symbol Map not set"));
    }

    public IEnumerable<IShape> Render()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IShape> RenderControls(IEnumerable<Control> controls) 
        => controls.SelectMany(RenderControl);
    public IEnumerable<IShape> RenderControl(Control control)
    {
        return control.Type switch
        {
            ControlType.Normal => RenderNormalControl(control),
            ControlType.Start => RenderStartControl(control),
            ControlType.Finish => RenderFinishControl(control),
            ControlType.CrossingPoint => RenderCrossingPointControl(control),
            ControlType.Exchange => Enumerable.Empty<IShape>(), // CourseRenderer2D.RenderCourse() handles this
            _ => throw new ArgumentException()
        };
    }
    public IEnumerable<IShape> RenderNormalControl(Control control)
    {
        PointSymbol sym = _activeEvent.SymbolMap!.Symbols["Control"] as PointSymbol
            ?? throw new InvalidOperationException("Finish should be a PointSymbol");

        int hash = sym.GetHashCode();
        if (_symbolCache.TryGetValue(hash, out IEnumerable<IShape>? value))
            return value.Select(x => { x.TopLeft = control.Position; return x; });

        var shapes = _mapRenderer.RenderPointSymbol(sym);
        _symbolCache.Add(hash, shapes);

        return shapes.Select(x => { x.TopLeft = control.Position; return x; });
    }
    public IEnumerable<IShape> RenderStartControl(Control control)
    {
        PointSymbol sym = _activeEvent.SymbolMap!.Symbols["Start"] as PointSymbol
            ?? throw new InvalidOperationException("Finish should be a PointSymbol");

        int hash = sym.GetHashCode();
        if (_symbolCache.TryGetValue(hash, out IEnumerable<IShape>? value))
            return value.Select(x => { x.TopLeft = control.Position; return x; });

        var shapes = _mapRenderer.RenderPointSymbol(sym);
        _symbolCache.Add(hash, shapes);

        return shapes.Select(x => { x.TopLeft = control.Position; return x; });
    }
    public IEnumerable<IShape> RenderFinishControl(Control control)
    {
        PointSymbol sym = _activeEvent.SymbolMap!.Symbols["Finish"] as PointSymbol 
            ?? throw new InvalidOperationException("Finish should be a PointSymbol");

        int hash = sym.GetHashCode();
        if (_symbolCache.TryGetValue(hash, out IEnumerable<IShape>? value))
            return value.Select(x => { x.TopLeft = control.Position; return x; });

        var shapes = _mapRenderer.RenderPointSymbol(sym);
        _symbolCache.Add(hash, shapes);

        return shapes.Select(x => { x.TopLeft = control.Position; return x; });
    }
    public IEnumerable<IShape> RenderCrossingPointControl(Control control)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IShape> RenderControlNumber(string text, vec2 controlPosition)
    {
        return Enumerable.Empty<IShape>();
    }

    public IEnumerable<IShape> RenderCourse(Course course)
    {
        return course switch
        {
            LinearCourse lc => RenderLinearCourse(lc),
            ScoreCourse sc => RenderScoreCourse(sc),
            _ => throw new ArgumentException()
        };
    }
    public IEnumerable<IShape> RenderLinearCourse(LinearCourse course)
    {
        throw new NotImplementedException();
    }
    public IEnumerable<IShape> RenderScoreCourse(ScoreCourse course)
    {
        ODebugger.Assert(!course.DisplayFormat.Contains("%n"));

        List<IShape> shapes = new();
        foreach (Control c in course.Controls)
        {
            string number = course.DisplayFormat
                .Replace("%c", c.Code.ToString())
                .Replace("%s", c.Score.ToString());

            shapes.AddRange(RenderControl(c));
            shapes.AddRange(RenderControlNumber(number, c.Position));
        }

        return shapes;
    }

    public IEnumerable<IShape> RenderCoursePart(ICoursePart part, string variation = "")
    {
        throw new NotImplementedException();
    }
    public IEnumerable<IShape> RenderLinearCoursePart(LinearCoursePart part, string variation = "")
    {
        List<IShape> shapes = new();

        for (int i = 1; i < part.Count; i++)
        {
            Control f = part[i - 1],
                t = part[i];

            vec4 line = new(f.Position, t.Position);

            float r1 = f.Type switch
            {
                ControlType.Normal => 0f,
                ControlType.Start => 0f,
                ControlType.Finish => 0f,
                ControlType.CrossingPoint => 0f,
                ControlType.Exchange => 0f,
            };

            float r2 = t.Type switch
            {
                ControlType.Normal => 0f,
                ControlType.Start => 0f,
                ControlType.Finish => 0f,
                ControlType.CrossingPoint => 0f,
                ControlType.Exchange => 0f,
            };

            line = _Utils.RemoveCircleFromLine(line, r1, r2);

            LineSymbol courseLineSym = _activeEvent.SymbolMap!.Symbols["CourseLine"] as LineSymbol
                ?? throw new InvalidOperationException("CourseLine should be a LineSymbol");

            PathCollection pC = new() { new LinearPath(new[] { line.XY, line.ZW }) };

            LineInstance lineInstance = new(0, courseLineSym, pC, false);

            shapes.AddRange(_mapRenderer.RenderPathInstance(lineInstance));

            shapes.AddRange(RenderControl(f));          
        }
    }
    public IEnumerable<IShape> RenderCombinedCoursePart(CombinedCoursePart part, string variation = "")
    {
        throw new NotImplementedException();
    }
    public IEnumerable<IShape> RenderVariationCoursePart(VariationCoursePart part, string variation = "")
    {
        throw new NotImplementedException();
    }
    public IEnumerable<IShape> RenderButterflyCoursePart(ButterflyCoursePart part, string variation = "")
    {
        throw new NotImplementedException();
    }
    public IEnumerable<IShape> RenderPhiLoopCoursePart(PhiLoopCoursePart part, string variation = "")
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

    public static float CalculateHeightOfEquilateralTriangle(float sideLength) 
        => .5f * (MathF.Sqrt(3) * sideLength);

    public static float ObjectExtrusion(PointSymbol sym)
    {
        float dist = float.MaxValue;

        foreach (var obj in sym.MapObjects)
        {
            switch(obj)
            {
                case PointObject obj:
                {
                    
                }
            }
        }
    }
    private static (vec2, float) MaxExtru(IEnumerable<vec2> points, vec2 centre)
    {
        vec2 nearest = vec2.Zero;
        float dist = float.MaxValue;

        foreach (vec2 p in points)
        {
            float mag = vec2.Mag(p, centre);

            if (mag < dist)
            {
                nearest = p;
                dist = mag;
            }
        }

        return (nearest, dist);
    }
}