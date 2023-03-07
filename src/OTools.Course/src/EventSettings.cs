using OTools.Maps;

namespace OTools.Course;

public class EventSettings
{
	public Colour OverprintColour { get; set; }
	public Colour DescriptionColour { get; set; }
	
	public ushort Scale { get; set; }
	
	public SizeMode Mode { get; set; }
	public ItemSizes ItemSizes { get; set; }

	public ControlNumberStyle NumberStyle { get; set; }
	public float WhiteOutline { get; set; }
	public float LegGap { get; set; }

	public static EventSettings Default => new()
	{
		OverprintColour = new("Overprint Purple", (35, 85, 0, 0)),
		DescriptionColour = new("Description Black", (0, 0, 0, 100)),
		
		Scale = 15000,
		
		Mode = SizeMode.IOF,
		ItemSizes = ItemSizes.ISOMStandard,
		
		NumberStyle = ControlNumberStyle.Regular,
		WhiteOutline = 0f,
		LegGap = 3.5f,
	};
	
	public enum SizeMode { IOF, Restricted, Free }
	public enum ControlNumberStyle { Regular, Bold }
}

public class ItemSizes
{
	public float StartSideLength { get; set; }
	public float ControlDiameter { get; set; }
	public float LineWidth { get; set; }
	public float DotDiameter { get; set; }
	public float ControlNumber { get; set; }
	public (float length, float width) MapIssueSize { get; set; }
	public (float length, float gap, float width) MarkedRoute { get; set; }
	public float OOBBoundaryWidth { get; set; }
	
	public static ItemSizes ISOMStandard => new()
	{
		StartSideLength = 6f,
		ControlDiameter = 5f,
		LineWidth = .35f,
		DotDiameter = 0f,
		ControlNumber = 4f,
		MapIssueSize = (2.5f, .6f),
		MarkedRoute = (2f, .5f, .35f),
		OOBBoundaryWidth = .7f,
	};

	public static ItemSizes ISSprOMStandard => new()
	{
		StartSideLength = 7f,
		ControlDiameter = 6f,
		LineWidth = .35f,
		DotDiameter = 0f,
		ControlNumber = 4f,
		MapIssueSize = (2.5f, .6f),
		MarkedRoute = (2f, .5f, .35f),
		OOBBoundaryWidth = 1f,
	};

	public static ItemSizes ISSkiOMStandard => new()
	{
		StartSideLength = 7f,
		ControlDiameter = 5.5f,
		LineWidth = .5f,
		DotDiameter = .65f,
		ControlNumber = 4f,
		MapIssueSize = (2.5f, .6f),
		MarkedRoute = (2f, .5f, .5f),
		OOBBoundaryWidth = 0f,
	};

	public static ItemSizes ISMTBOMStandard => new()
	{
		StartSideLength = 7f,
		ControlDiameter = 6f,
		LineWidth = .6f,
		DotDiameter = 0f,
		ControlNumber = 4f,
		MapIssueSize = (2.5f, .6f),
		MarkedRoute = (2f, .5f, .6f),
		OOBBoundaryWidth = 1f,
	};

	public static ItemSizes ISMTBOMStandard_Alternative => new()
	{
		StartSideLength = 7f,
		ControlDiameter = 6f,
		LineWidth = .6f,
		DotDiameter = .6f,
		ControlNumber = 4f,
		MapIssueSize = (2.5f, .6f),
		MarkedRoute = (2f, .5f, .6f),
		OOBBoundaryWidth = 1f,
	};

}