using OTools.AvaCommon;
using OTools.Courses;
using OTools.ObjectRenderer2D;

namespace OTools.CoursePlanner;

public static class Manager
{
	public static PaintBox? PaintBox { get; set; }
	public static Event Event { get; set; }
	
	//public static ICourseRenderer2D? CourseRenderer { get; set; }
	public static CoursePlannerSettings Settings { get; set; } = CoursePlannerSettings.Default;
}