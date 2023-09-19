using System;
using System.Threading;

namespace OTools.MapMaker;

public class MapMakerSettings
{
	public float Draw_Opacity { get; set; }
	
	public uint Draw_BorderColour { get; set; }
	public float Draw_BorderWidth { get; set; }
	public int Draw_BorderZIndex { get; set; }

	public float Select_BBoxOffset { get; set; }
	public float Select_BBoxLineWidth { get; set; }
	public double[] Select_BBoxDashArray { get; set; } = Array.Empty<double>();
	public uint Select_BBoxColour { get; set; }
	public int Select_BBoxZIndex { get; set; }

	public float Select_HandleRadius { get; set; }
	public float Select_HandleLineWidth { get; set; }
	public uint Select_HandleAnchorColour { get; set; }
	public uint Select_HandleControlColour { get; set; }
	public int Select_HandleZIndex { get; set; }

	public float Select_ObjectTolerance { get; set; }
	public float Select_PointTolerance { get; set; }

	public static MapMakerSettings Default => new()
	{
		Draw_Opacity = .5f,
		
		Draw_BorderColour = 0xffff8a00,
		Draw_BorderWidth = .5f,
		Draw_BorderZIndex = 999,

		Select_BBoxOffset = 2f,
		Select_BBoxLineWidth = .5f,
		Select_BBoxDashArray = new double[] { 30, 5 },
		Select_BBoxColour = 0xffffa600,
		Select_BBoxZIndex = 998,

		Select_HandleRadius = 4f,
		Select_HandleLineWidth = 1.2f,
		Select_HandleAnchorColour = 0xff000000,
		Select_HandleControlColour = 0xffffa600,
		Select_HandleZIndex = 999,

		Select_ObjectTolerance = 2f,
		Select_PointTolerance = 6f,
	};

}