using OTools.Common;

namespace OTools.Maps;

//TODO: Implement GetHashCode()

[DebuggerDisplay("{Name}, {Number}")]
public abstract class Symbol : IStorable
{
	public Guid Id { get; init; }

    public string Name { get; set; }

    public string Description { get; set; }

    public SymbolNumber Number { get; set; }

    public bool IsUncrossable { get; set; }

    public bool IsHelperSymbol { get; set; }

    // Icon

    protected Symbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol)
    {
		Id = Guid.NewGuid();

        Name = name;
        Description = description;
        Number = number;

        IsUncrossable = isUncrossable;
        IsHelperSymbol = isHelperSymbol;
    }

    protected Symbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol)
    {
        Id = id;

        Name = name;
        Description = description;
        Number = number;

        IsUncrossable = isUncrossable;
        IsHelperSymbol = isHelperSymbol;
    }
}

[DebuggerDisplay("{First}-{Second}-{Third}")]
public struct SymbolNumber
{
    public byte First { get; set; }
    public byte Second { get; set; }
    public byte Third { get; set; }

    public SymbolNumber(byte first, byte second, byte third)
    {
        First = first;
        Second = second;
        Third = third;
    }

    public SymbolNumber(string str)
    {
        string[] split = str.Split('-');

        byte.TryParse(split[0], out byte first);
        byte.TryParse(split[1], out byte second);
        byte.TryParse(split[2], out byte third);

        First = first;
        Second = second;
        Third = third;
    }

    public static implicit operator SymbolNumber((byte, byte, byte) tup)
        => new(tup.Item1, tup.Item2, tup.Item3);
}

public interface IPathSymbol
{
    Guid Id { get; }

    DashStyle DashStyle { get; set; }

    MidStyle MidStyle { get; set; }
    
    LineStyle LineStyle { get; set; }

    BorderStyle BorderStyle { get; set; }

    float Width { get; set; }
    Colour Colour { get; set; }
}

[DebuggerDisplay("Dash Style {HasDash} - {DashLength}, {GapLength}")]
public struct DashStyle
{
    public bool HasDash { get; set; }

    public float DashLength { get; set; }

    public float GapLength { get; set; }

    public int GroupSize { get; set; }

    public float GroupGapLength { get; set; }

    public DashStyle()
    {
        HasDash = false;

        DashLength = 0f;
        GapLength = 0f;
        GroupSize = 0;
        GroupGapLength = 0f;
    }

    public DashStyle(float dashLength, float gapLength, int groupSize = 0, float groupGapLength = 0)
    {
        HasDash = true;

        DashLength = dashLength;
        GapLength = gapLength;
        GroupSize = groupSize;
        GroupGapLength = groupGapLength;
    }

    public static DashStyle None => new();
}

[DebuggerDisplay("Mid Style {HasMid} - {GapLength}")]
public struct MidStyle
{
    public bool HasMid { get; set; }

    public List<MapObject> MapObjects { get; set; }

    public float GapLength { get; set; }

    public bool RequireMid { get; set; }

    public float InitialOffset { get; set; }

    public float EndOffset { get; set; }

    public MidStyle()
    {
        HasMid = false;

        MapObjects = new();

        GapLength = 0;
        RequireMid = false;

        InitialOffset = 0;
        EndOffset = 0;
    }

    public MidStyle(IEnumerable<MapObject> mapObjects, float gapLength, bool requireMid, float initialOffset, float endOffset)
    {
        HasMid = true;

        MapObjects = new(mapObjects);

        GapLength = gapLength;
        RequireMid = requireMid;

        InitialOffset = initialOffset;
        EndOffset = endOffset;
    }

    public static MidStyle None => new();
}

[DebuggerDisplay("Line Style {Join} - {Cap}")]
public struct LineStyle
{
    public JoinStyle Join { get; set; }
    public CapStyle Cap { get; set; }

    public LineStyle(JoinStyle join, CapStyle cap)
    {
        Join = join;
        Cap = cap;
    }

    public LineStyle(int join, int cap)
    {
        Join = (JoinStyle)join;
        Cap = (CapStyle)cap;
    }

    public static LineStyle Default => new(0, 0);

    public enum JoinStyle { Flat, Round, Square }
    public enum CapStyle { Bevel, Miter, Round }
}

public struct BorderStyle
{
    public bool HasBorder { get; set; }
    
    public Colour Colour { get; set; }
    
    public float Width { get; set; }
    
    public float Offset { get; set; }
    
    public DashStyle DashStyle { get; set; }
    
    public MidStyle MidStyle { get; set; }

    public BorderStyle()
    {
        HasBorder = false;

        Colour = Colour.Transparent;
        Width = 0f;
        Offset = 0f;
        
        DashStyle = DashStyle.None;
        MidStyle = MidStyle.None;
    }

    public BorderStyle(Colour colour, float width, float offset, DashStyle? dashStyle = null, MidStyle? midStyle = null)
    {
        HasBorder = true;

        Colour = colour;
        Width = width;
        Offset = offset;

        DashStyle = dashStyle is null ? DashStyle.None : (DashStyle)dashStyle!;
        MidStyle = midStyle is null ? MidStyle.None : (MidStyle)midStyle!;
    }

    public static BorderStyle None => new();
}