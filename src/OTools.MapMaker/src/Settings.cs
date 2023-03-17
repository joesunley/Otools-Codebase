using System.Threading;

namespace OTools.MapMaker;

public class MapMakerSettings
{
    public float Draw_Opacity { get; set; }

    public static MapMakerSettings Default => new()
    {
        Draw_Opacity = .5f,
    };
}