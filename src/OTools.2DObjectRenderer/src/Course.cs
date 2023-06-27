using OTools.Courses;
using OTools.Maps;
using Sunley.Mathematics;

namespace OTools.ObjectRenderer2D;

public static partial class RenderExtension
{
    public static IEnumerable<IShape> Render(this Control control)
    {
        throw new NotImplementedException();
    }

	// public static Line Render(this CourseLine line)
	// {
	// 	Line output = new()
	// 	{
	// 		Points = new()
	// 		{
	// 			line.Start, line.End,
	// 		},
	// 		
	// 		Colour = 
	// 	};
	// }
}

file static class _Render
{
    private static Map? s_activeMap;
	private static Event? s_activeCourse;

	public static void SetActiveMap(Map map) => s_activeMap = map;

	public static Line RenderCourseLine(CourseLine line)
	{
		Colour col;
		if (line.Colour is not null)
			col = line.Colour;
		else
		{
			try
			{
				col = s_activeMap.Colours.First(x => x.Name.ToLower().Contains("overprint"));
			}
			catch
			{
				col = new CmykColour("Overprint", 35, 85, 0, 0);
			}
		}

		return new()
		{
			Points = new() { line.Start, line.End },

			Colour = col,
		};
	}
}

file static class _SymbolDefinitions
{
    public const string StartTriange = """

        """;
}

public class CourseLine
{
	private vec4 _line;

	public vec2 Start
	{
		get => _line.XY;

		set
		{
			_line.X = value.X;
			_line.Y = value.Y;
		}
	}

	public vec2 End
	{
		get => _line.ZW;

		set
		{
			_line.X = value.X;
			_line.Y = value.Y;
		}
	}

	public Colour? Colour { get; set; }
	
	public CourseLine(vec2 start, vec2 end)
	{
		_line = new(start, end);
	}

	public CourseLine(vec4 line)
	{
		_line = line;
	}
}