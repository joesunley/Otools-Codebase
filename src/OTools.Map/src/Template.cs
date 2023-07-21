using OTools.Common;
using SixLabors.ImageSharp;

namespace OTools.Maps;

public abstract class Template : IStorable
{
    public Guid Id { get; init; }
    
    public bool IsVisible { get; set; }
    public string FilePath { get; set; }
    public TemplateAdjustment Adjustment { get; set; }

    protected Template(bool isVisible, TemplateAdjustment adjustment, string filePath)
    {
        Id = Guid.NewGuid();

        IsVisible = isVisible;
        FilePath = filePath;
        Adjustment = adjustment;
    }

    protected Template(Guid id, bool isVisible, TemplateAdjustment adjustment, string filePath)
    {
        Id = id;

        IsVisible = isVisible;
        FilePath = filePath;
        Adjustment = adjustment;
    }
}

public class MapTemplate : Template
{
    public Map Map { get; set; }

    public MapTemplate(bool isVisible, Map map, TemplateAdjustment adjustment, string filePath)
        : base(isVisible, adjustment, filePath)
    {
        Map = map;
    }

    public MapTemplate(Guid id, bool isVisible, Map map, TemplateAdjustment adjustment, string filePath)
        : base(id, isVisible, adjustment, filePath)
    {
        Map = map;
    }
}

public class BitmapTemplate 
{

  
}

public struct TemplateAdjustment
{
    public vec2 Position { get; set; }
    public vec2 Scale { get; set; }
    public float Rotation { get; set; } // Radians

	public static TemplateAdjustment None => new();
}