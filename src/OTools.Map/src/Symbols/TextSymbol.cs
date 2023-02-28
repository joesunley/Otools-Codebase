namespace OTools.Maps;

public sealed class TextSymbol : Symbol
{
    public Font Font { get; set; }

    public bool IsRotatable { get; set; }

    public Colour BorderColour { get; set; }
    public float BorderWidth { get; set; }

    public Colour FramingColour { get; set; }
    public float FramingWidth { get; set; }

    public TextSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Font font, bool isRotatable, Colour borderColour, float borderWidth, Colour framingColour, float framingWidth)
        : base(name, description, number, isUncrossable, isHelperSymbol)
    {
        Font = font;
        IsRotatable = isRotatable;
        BorderColour = borderColour;
        BorderWidth = borderWidth;
        FramingColour = framingColour;
        FramingWidth = framingWidth;
    }
    public TextSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Font font, bool isRotatable, Colour borderColour, float borderWidth, Colour framingColour, float framingWidth)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)
    {
        Font = font;
        IsRotatable = isRotatable;
        BorderColour = borderColour;
        BorderWidth = borderWidth;
        FramingColour = framingColour;
        FramingWidth = framingWidth;
    }

    public TextSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable,
                      bool isHelperSymbol, Font font, bool isRotatable,
                      (Colour col, float width)? border, (Colour col, float width)? framing)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)
    {
        Font = font;
        IsRotatable = isRotatable;

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