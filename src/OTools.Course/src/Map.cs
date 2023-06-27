using OTools.Maps;
using Adjustment = OTools.Maps.TemplateAdjustment;

namespace OTools.CoursePlanner;

public abstract class CourseMap
{
	public string FilePath { get; set; }
	
	public Adjustment Adjustment { get; set; }

	protected CourseMap(string filePath, Adjustment adjustment)
	{
		FilePath = filePath;
		Adjustment = adjustment;
	}
}

public class OMap : CourseMap
{
	public Map Map { get; set; }
	
	public OMap(string filePath, Adjustment adjustment, Map map)
		: base(filePath, adjustment)
	{
		Map = map;
	}
}

public class BitmapMap
{
	
}

public class VectorMap
{
	
}

