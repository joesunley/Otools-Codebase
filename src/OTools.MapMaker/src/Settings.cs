using System.Threading;

namespace OTools.MapMaker;

public class MapMakerSettings
{
    public float Draw_Opacity { get; set; }
	
	public uint Draw_BorderColour { get; set; }
	public float Draw_BorderWidth { get; set; }
	public int Draw_BorderZIndex { get; set; }

    public static MapMakerSettings Default => new()
    {
        Draw_Opacity = .5f,
		
		Draw_BorderColour = 0xffff8a00,
		Draw_BorderWidth = .5f,
		Draw_BorderZIndex = 999,
    };
}