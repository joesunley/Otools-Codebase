namespace OTools.Maps;

public sealed class AreaSymbol : Symbol, IPathSymbol
{
    public IFill Fill { get; set; }

    public Colour BorderColour { get; set; }

    public float BorderWidth { get; set; }

    public DashStyle DashStyle { get; set; }

    public MidStyle MidStyle { get; set; }

    public bool RotatablePattern { get; set; }

    public AreaSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, IFill fill, Colour borderColour, float width, DashStyle dashStyle, MidStyle midStyle, bool rotatablePattern)
        : base(name, description, number, isUncrossable, isHelperSymbol)
    {
        Fill = fill;
        BorderColour = borderColour;
        BorderWidth = width;
        DashStyle = dashStyle;
        MidStyle = midStyle;
        RotatablePattern = rotatablePattern;
    }

    public AreaSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, IFill fill, Colour borderColour, float width, DashStyle dashStyle, MidStyle midStyle, bool rotatablePattern)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)
    {
        Fill = fill;
        BorderColour = borderColour;
        BorderWidth = width;
        DashStyle = dashStyle;
        MidStyle = midStyle;
        RotatablePattern = rotatablePattern;
    }
}
