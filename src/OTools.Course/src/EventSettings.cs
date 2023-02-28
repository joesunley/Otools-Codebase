using OTools.Maps;

namespace OTools.Course;

public class EventSettings
{
	public Colour OverprintColour { get; set; }
	public Colour DescriptionColour { get; set; }
	
	public SizeMode Mode { get; set; }

	public ControlNumberStyle NumberStyle { get; set; }
	public float WhiteOutline { get; set; }
	public float LegGap { get; set; }
	// public ScaleMode ItemScaleMode { get; set; }

	public static EventSettings Default => new()
	{
		OverprintColour = new("Overprint Purple", (35, 85, 0, 0)),
		DescriptionColour = new("Description Black", (0, 0, 0, 100)),
		
		Mode = SizeMode.IOF,
		// ItemScaleMode = ScaleMode.Fifteen,
	};
	
	public enum SizeMode { IOF, RestrictedRatio, Free }
	public enum ControlNumberStyle { Regular, Bold }
	public enum ScaleMode { None, MapScale, Fifteen }
}

public class ItemSizes
{
	public float ControlDiameter { get; set; }
	public float LineWidth { get; set; }
	public float CentreDot { get; set; }
	public float ControlNumber { get; set; }
	
	public (float length, float width) MapIssueSize { get; set; }


	public static ItemSizes ISOMStandard => new()
	{
		ControlDiameter = 5f,
		LineWidth = .35f,
		CentreDot = 0f,
		ControlNumber = 4f,
	};
}