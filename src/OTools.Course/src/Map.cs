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

public class BlankMap : CourseMap
{
	public BlankMap() : base(string.Empty, Adjustment.None) { }
}

public class OMap : CourseMap
{
	public Map Map { get; set; }
	public FileFormat Format { get; set; }
	
	public OMap(string filePath, Adjustment adjustment, Map map, FileFormat format)
		: base(filePath, adjustment)
	{
		Map = map;
		Format = format;
	}

	public enum FileFormat { Internal, OOM, OCAD }
}

public class BitmapMap
{
	public float DPI { get; set; }
	public float Scale { get; set; }
}

public class VectorMap
{
	
}

