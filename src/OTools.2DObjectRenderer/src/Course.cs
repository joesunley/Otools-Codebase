using System.Diagnostics;
using OTools.Courses;
using OTools.Maps;
using ownsmtp.logging;
using Sunley.Mathematics;
using System.Text;
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

    private readonly MapRenderer2D _mapRenderer;

    public CourseRenderer2D(Event ev)
    {
        _activeEvent = ev;
        _mapRenderer = new(_activeEvent.SymbolMap 
            ?? throw new Exception("Symbol Map not set"));
    }

    public IEnumerable<IShape> Render()
	{
		throw new InvalidOperationException();
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
            _ => throw new ArgumentException(),
        };
    }
    public IEnumerable<IShape> RenderNormalControl(Control control)
    {
        PointSymbol sym = _activeEvent.SymbolMap!.Symbols["Control"] as PointSymbol
            ?? throw new InvalidOperationException("Finish should be a PointSymbol");

		var shapes = _mapRenderer.RenderPointSymbol(sym);

		return shapes.Select(x => { x.TopLeft = control.Position; return x; });
	}
    public IEnumerable<IShape> RenderStartControl(Control control)
    {
        PointSymbol sym = _activeEvent.SymbolMap!.Symbols["Start"] as PointSymbol
            ?? throw new InvalidOperationException("Finish should be a PointSymbol");

        var shapes = _mapRenderer.RenderPointSymbol(sym);

        return shapes.Select(x => { x.TopLeft = control.Position; return x; });
    }
    public IEnumerable<IShape> RenderFinishControl(Control control)
    {
        PointSymbol sym = _activeEvent.SymbolMap!.Symbols["Finish"] as PointSymbol 
            ?? throw new InvalidOperationException("Finish should be a PointSymbol");

		var shapes = _mapRenderer.RenderPointSymbol(sym);

		return shapes.Select(x => { x.TopLeft = control.Position; return x; });

	}
    public IEnumerable<IShape> RenderCrossingPointControl(Control control)
    {
        throw new NotImplementedException();
    }

	public IEnumerable<IShape> RenderControlNumbers(IEnumerable<Control> controls, string displayFormat)
	{
		return controls.SelectMany(c =>
		{
			string number = displayFormat
							  .Replace("%c", c.Code.ToString())
							  .Replace("%s", c.Score.ToString());

			return RenderControlNumber(number, c.Position);
		});
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
            _ => throw new ArgumentException(),
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
        return part switch
        {
            LinearCoursePart lcp => RenderLinearCoursePart(lcp, variation),
            CombinedCoursePart ccp => RenderCombinedCoursePart(ccp, variation),
            VariationCoursePart vcp => RenderVariationCoursePart(vcp, variation),
            ButterflyCoursePart bcp => RenderButterflyCoursePart(bcp, variation),
            PhiLoopCoursePart plcp => RenderPhiLoopCoursePart(plcp, variation),
            _ => throw new ArgumentException()
        };
    }
    public IEnumerable<IShape> RenderLinearCoursePart(LinearCoursePart part, string variation = "") 
        => RenderLinearCoursePart(part);

    public IEnumerable<IShape> RenderLinearCoursePart(IList<Control> controls)
    {
        List<IShape> shapes = new();

        for (int i = 1; i < controls.Count; i++)
        {
            Control f = controls[i - 1],
                t = controls[i];

            vec4 line = new(f.Position, t.Position);

            float r1 = _Utils.CalculateLineEscape(f.Type, _activeEvent.SymbolMap!.Symbols);
            float r2 = _Utils.CalculateLineEscape(t.Type, _activeEvent.SymbolMap!.Symbols);


            line = _Utils.RemoveCircleFromLine(line, r1, r2);

            LineSymbol courseLineSym = _activeEvent.SymbolMap!.Symbols["CourseLine"] as LineSymbol
                ?? throw new InvalidOperationException("CourseLine should be a LineSymbol");

            PathCollection pC = new() { new LinearPath(new[] { line.XY, line.ZW }) };

            LineInstance lineInstance = new(0, courseLineSym, pC, false);

            shapes.AddRange(_mapRenderer.RenderPathInstance(lineInstance));

            shapes.AddRange(RenderControl(f));

            if (i == controls.Count - 1)
                shapes.AddRange(RenderControl(t));
        }

        return shapes;

    }
    public IEnumerable<IShape> RenderCombinedCoursePart(CombinedCoursePart part, string variation = "")
    {
        List<IShape> shapes = new();
        foreach (var p in part)
            shapes.AddRange(RenderCoursePart(p, variation));
        return shapes;
    }
    public IEnumerable<IShape> RenderVariationCoursePart(VariationCoursePart part, string variation = "")
    {
        if (variation == "all")
        {
            throw new NotImplementedException();
        }
        else if (variation.Contains('{') || variation.Contains('}'))
        {
            throw new NotImplementedException();
        }
        else // Simple variation
		{
			int index = variation.Parse<int>();

            ICoursePart coursePart = part.Parts[index];
            ODebugger.Assert(coursePart is LinearCoursePart);

            List<Control> controls = (coursePart as LinearCoursePart)!;
            controls.Insert(0, part.First);
            controls.Add(part.Last);

            return RenderLinearCoursePart(controls);
        }
        
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

    public static float CalculateLineEscape(ControlType controlType, SymbolStore symbols)
    {
        PointSymbol sym = controlType switch
        {
            ControlType.Normal => symbols["Control"] as PointSymbol
                                ?? throw new InvalidOperationException("Control should be a PointSymbol"),
            ControlType.Start => symbols["Start"] as PointSymbol
                                ?? throw new InvalidOperationException("Start should be a PointSymbol"),
            ControlType.Finish => symbols["Finish"] as PointSymbol
                                ?? throw new InvalidOperationException("Finish should be a PointSymbol"),
            ControlType.CrossingPoint => symbols["CrossingPoint"] as PointSymbol
                                ?? throw new InvalidOperationException("CrossingPoint should be a PointSymbol"),
            ControlType.Exchange => symbols["Exchange"] as PointSymbol
                                ?? throw new InvalidOperationException("Exchange should be a PointSymbol"),
            _ => throw new InvalidOperationException(),
        };

        return ObjectExtrusion(sym);
    }

    public static void FilterVarStr(string variation)
    {
        foreach (char c in variation)
        {

        }
    }
}