namespace OTools.Maps;

public sealed class AreaSymbol : Symbol, IPathSymbol
{
    public IFill Fill { get; set; }

    public Colour Colour { get; set; }

    public float Width { get; set; }

    public DashStyle DashStyle { get; set; }

    public MidStyle MidStyle { get; set; }

    public LineStyle LineStyle { get; set; }

    public BorderStyle BorderStyle { get; set; }

    public bool RotatablePattern { get; set; }

    public AreaSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, IFill fill, Colour colour, float width, DashStyle dashStyle, MidStyle midStyle, LineStyle lineStyle, BorderStyle borderStyle, bool rotatablePattern)
        : base(name, description, number, isUncrossable, isHelperSymbol)
    {
        Fill = fill;
        Colour = colour;
        Width = width;
        DashStyle = dashStyle;
        MidStyle = midStyle;
        LineStyle = lineStyle;
        BorderStyle = borderStyle;
        RotatablePattern = rotatablePattern;
    }

    public AreaSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, IFill fill, Colour colour, float width, DashStyle dashStyle, MidStyle midStyle, LineStyle lineStyle, BorderStyle borderStyle, bool rotatablePattern)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)
    {
        Fill = fill;
        Colour = colour;
        Width = width;
        DashStyle = dashStyle;
        MidStyle = midStyle;
        LineStyle = lineStyle;
        BorderStyle = borderStyle;
        RotatablePattern = rotatablePattern;
    }
}
