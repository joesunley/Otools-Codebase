using OTools.Common;

namespace OTools.Maps;

public abstract class MapObject : IStorable
{
    public Guid Id { get; set; }

    protected MapObject()
    {
        Id = Guid.NewGuid();
    }

    protected MapObject(Guid id)
    {
        Id = id;
    }
}

public sealed class PointObject : MapObject
{
    public Colour InnerColour { get; set; }
    public Colour OuterColour { get; set; }

    public float InnerRadius { get; set; }
    public float OuterRadius { get; set; }

    public PointObject(Colour innerColour, Colour outerColour, float innerRadius, float outerRadius) : base()
    {
        InnerColour = innerColour;
        OuterColour = outerColour;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
    }

    public PointObject(Guid id, Colour innerColour, Colour outerColour, float innerRadius, float outerRadius) : base(id)
    {
        InnerColour = innerColour;
        OuterColour = outerColour;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
    }
}

public sealed class LineObject : MapObject
{
    public PathCollection Segments { get; set; }

    public float Width { get; set; }
    public Colour Colour { get; set; }

    public LineObject(PathCollection segments, float width, Colour colour) : base()
    {
        Segments = segments;
        Width = width;
        Colour = colour;
    }

    public LineObject(Guid id, PathCollection segments, float width, Colour colour) : base(id)
    {
        Segments = segments;
        Width = width;
        Colour = colour;
    }
}

public sealed class AreaObject : MapObject
{
    public PathCollection Segments { get; set; }

    public float BorderWidth { get; set; }

    public Colour BorderColour { get; set; }
    public IFill Fill { get; set; }

    public AreaObject(PathCollection segments, float borderWidth, Colour borderColour, IFill fill) : base()
    {
        Segments = segments;
        BorderWidth = borderWidth;
        BorderColour = borderColour;
        Fill = fill;
    }

    public AreaObject(Guid id, PathCollection segments, float borderWidth, Colour borderColour, IFill fill) : base(id)
    {
        Segments = segments;
        BorderWidth = borderWidth;
        BorderColour = borderColour;
        Fill = fill;
    }
}

public sealed class TextObject : MapObject
{
    public string Text { get; set; }
    public vec2 TopLeft { get; set; }
    public float Rotation { get; set; }

    public Font Font { get; set; }

    public Colour BorderColour { get; set; }
    public float BorderWidth { get; set; }

    public Colour FramingColour { get; set; }
    public float FramingWidth { get; set; }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public TextObject(string text, vec2 topLeft, float rotation, Font font, Colour borderColour, float borderWidth, Colour framingColour, float framingWidth, HorizontalAlignment horizontalAlignment) : base()
    {
        Text = text;
        TopLeft = topLeft;
        Rotation = rotation;
        Font = font;
        BorderColour = borderColour;
        BorderWidth = borderWidth;
        FramingColour = framingColour;
        FramingWidth = framingWidth;
        HorizontalAlignment = horizontalAlignment;
    }

    public TextObject(Guid id, string text, vec2 topLeft, float rotation, Font font, (Colour col, float width)? border, (Colour col, float width)? framing, HorizontalAlignment horizontalAlignment) : base(id)
    {
        Text = text;
        TopLeft = topLeft;
        Rotation = rotation;
        Font = font;
        HorizontalAlignment = horizontalAlignment;

        if (border is null)
        {
            BorderColour = Colour.Transparent;
            BorderWidth = 0;
        }
        else
        {
            BorderColour = border.Value.col;
            BorderWidth = border.Value.width;
        }

        if (framing is null)
        {
            FramingColour = Colour.Transparent;
            FramingWidth = 0;
        }
        else
        {
            FramingColour = framing.Value.col;
            FramingWidth = framing.Value.width;
        }
    }
}