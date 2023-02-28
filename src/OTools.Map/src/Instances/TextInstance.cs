using OTools.Maps;

namespace OTools.Maps;

public sealed class TextInstance : Instance<TextSymbol>
{
    public string Text { get; set; }

    public vec2 TopLeft { get; set; }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public float Rotation { get; set; }

    public TextInstance(int layer, TextSymbol symbol, string text, vec2 topLeft, HorizontalAlignment horizontalAlignment, float rotation)
        : base(layer, symbol)
    {
        Text = text;
        TopLeft = topLeft;
        HorizontalAlignment = horizontalAlignment;
        Rotation = rotation;
    }

    public TextInstance(Guid id, int layer, TextSymbol symbol, string text, vec2 topLeft, HorizontalAlignment horizontalAlignment, float rotation)
        : base(id, layer, symbol)
    {
        Text = text;
        TopLeft = topLeft;
        HorizontalAlignment = horizontalAlignment;
        Rotation = rotation;
    }
}

public enum HorizontalAlignment { Left, Centre, Right, Justify }
