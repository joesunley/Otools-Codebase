namespace OTools.Maps;

public sealed class Font
{
    public string FontFamily { get; set; }

    public Colour Colour { get; set; }

    public float Size { get; set; } // Size in mm

    public float LineSpacing { get; set; }
    public float ParagraphSpacing { get; set; }
    public float CharacterSpacing { get; set; }

    public FontStyle FontStyle { get; set; }

    public Font(string fontFamily, Colour colour, float size, float lineSpacing, float paragraphSpacing, float characterSpacing, FontStyle fontStyle)
    {
        FontFamily = fontFamily;
        Colour = colour;
        Size = size;
        LineSpacing = lineSpacing;
        ParagraphSpacing = paragraphSpacing;
        CharacterSpacing = characterSpacing;
        FontStyle = fontStyle;
    }

    private const float POINTSTOMILLIMETRE_CONVERSION_FACTOR = 0.352777778f;

    public static float ConvertPointsToMillimeters(float points)
    {
        return points * POINTSTOMILLIMETRE_CONVERSION_FACTOR;
    }
    public static float ConvertMillimetersToPoints(float millimeters)
    {
        return millimeters / POINTSTOMILLIMETRE_CONVERSION_FACTOR;
    }
}

public struct FontStyle
{
    public bool Bold { get; set; }
    public bool Underline { get; set; }
    public bool Strikeout { get; set; }
    public ItalicsMode Italics { get; set; }

    public FontStyle(bool bold, bool underline, bool strikeout, ItalicsMode italics)
    {
        Bold = bold;
        Underline = underline;
        Strikeout = strikeout;
        Italics = italics;
    }

    public static FontStyle None => new(false, false ,false, ItalicsMode.None); 
}

public enum ItalicsMode { None, Italic, Oblique }