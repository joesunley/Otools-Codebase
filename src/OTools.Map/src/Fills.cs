namespace OTools.Maps;

public interface IFill { }

public sealed class SolidFill : IFill
{
    public Colour Colour { get; set; }

    public SolidFill(Colour colour)
    {
        Colour = colour;
    }

    public static implicit operator SolidFill(Colour colour)
        => new(colour);
    public static implicit operator Colour(SolidFill fill)
        => fill.Colour;
}

public abstract class ObjectFill : IFill
{
    public List<MapObject> Objects { get; set; }

    public bool IsClipped { get; set; }

    protected ObjectFill()
    {
        Objects = new();
    }
}

public sealed class RandomObjectFill : ObjectFill
{
    public float Spacing { get; set; }

    public RandomObjectFill(IEnumerable<MapObject> mapObjects, bool isClipped, float spacing)
    {
        Objects = new(mapObjects);
        IsClipped = isClipped;
        Spacing = spacing;
    }
}

public sealed class SpacedObjectFill : ObjectFill
{
    public vec2 Spacing { get; set; }

    public vec2 Offset { get; set; }

    public float Rotation { get; set; }

    public SpacedObjectFill(IEnumerable<MapObject> mapObjects, bool isClipped, vec2 spacing, vec2 offset, float rotation)
    {
        Objects = new(mapObjects);
        IsClipped = isClipped;
        Spacing = spacing;
        Offset = offset;
        Rotation = rotation;
    }
}

public sealed class PatternFill : IFill
{
    public float ForeSpacing { get; set; }
    public float BackSpacing { get; set; }

    public Colour ForeColour { get; set; }
    public Colour BackColour { get; set; }

    public float Rotation { get; set; } // Radians

    public PatternFill(float foreSpacing, float backSpacing, Colour foreColour, Colour backColour, float rotation)
    {
        ForeSpacing = foreSpacing;
        BackSpacing = backSpacing;

        ForeColour = foreColour;
        BackColour = backColour;

        Rotation = rotation;
    }
}

public sealed class CombinedFill : IFill
{
    public List<IFill> Fills { get; set; }

    public CombinedFill(IEnumerable<IFill> fills)
    {
        Fills = new(fills);
    }
}