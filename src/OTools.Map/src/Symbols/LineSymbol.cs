namespace OTools.Maps;

public sealed class LineSymbol : Symbol, IPathSymbol
{
    public Colour BorderColour { get; set; }

    public float BorderWidth { get; set; }

    public DashStyle DashStyle { get; set; }

    public MidStyle MidStyle { get; set; }

    public LineSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Colour borderColour, float width, DashStyle dashStyle, MidStyle midStyle)
        : base(name, description, number, isUncrossable, isHelperSymbol)
    {
        BorderColour = borderColour;
        BorderWidth = width;
        DashStyle = dashStyle;
        MidStyle = midStyle;
    }
    public LineSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Colour borderColour, float width, DashStyle dashStyle, MidStyle midStyle)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)
    {
        BorderColour = borderColour;
        BorderWidth = width;
        DashStyle = dashStyle;
        MidStyle = midStyle;
    }
}