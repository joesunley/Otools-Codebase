using OTools.Common;

namespace OTools.Courses;

public sealed class Control : IStorable
{
    public Guid Id { get; set; }

    public ushort Code { get; set; }
    public vec2 Position { get; set; }  
    public ControlType Type { get; set; }
    public Description Description { get; set; }

    public ControlGaps Gaps { get; set; }

    public Control(ushort code, vec2 position, ControlType type, Description description, ControlGaps? gaps)
    {
        Id = Guid.NewGuid();

        Code = code;
        Position = position;
        Type = type;
        Description = description;
        
        Gaps = gaps ?? new();
    }

    public Control(Guid id, ushort code, vec2 position, ControlType type, Description description, ControlGaps? gaps)
    {
        Id = id;
        Code = code;
        Position = position;
        Type = type;
        Description = description;

        Gaps = gaps ?? new();
    }
}

public sealed class ControlGaps : List<(float startRad, float endRad)>
{
    public ControlGaps() : base() { }
    public ControlGaps(IEnumerable<(float, float)> collection) : base(collection) { }
    public ControlGaps(int capacity) : base(capacity) { }
}

[Flags]
public enum ControlType : byte
{
    Normal        = 0b00001,
    Start         = 0b00010,
    Finish        = 0b00100,
    CrossingPoint = 0b01000,
    Exchange      = 0b10000,
}

public struct Description
{
    public ColumnC ColumnC { get; set; }
    public ColumnD ColumnD { get; set; }
    public ColumnE ColumnE { get; set; }
    public ColumnF ColumnF { get; set; }
    public ColumnG ColumnG { get; set; }
    public ColumnH ColumnH { get; set; }
    public DescriptionDetail? ColumnFDetail { get; set; }

    public Description()
    {
        ColumnC = ColumnC.NONE;
        ColumnD = ColumnD.NONE;
        ColumnE = ColumnE.NONE;
        ColumnF = ColumnF.NONE;
        ColumnG = ColumnG.NONE;
        ColumnH = ColumnH.NONE;
    }

    public Description(ColumnC columnC, ColumnD columnD, ColumnE columnE, ColumnF columnF, ColumnG columnG, ColumnH columnH, DescriptionDetail? columnFDetail = null)
    {
        ColumnC = columnC;
        ColumnD = columnD;
        ColumnE = columnE;
        ColumnF = columnF;
        ColumnG = columnG;
        ColumnH = columnH;
        ColumnFDetail = columnFDetail;
    }
    public Description(byte columnC, byte columnD, byte columnE, byte columnF, byte columnG, byte columnH, DescriptionDetail? columnFDetail = null)
    {
        ColumnC = (ColumnC)columnC;
        ColumnD = (ColumnD)columnD;
        ColumnE = (ColumnE)columnE;
        ColumnF = (ColumnF)columnF;
        ColumnG = (ColumnG)columnG;
        ColumnH = (ColumnH)columnH;

        ColumnFDetail = columnFDetail;
    }

    public static Description Empty => new();
}

public record struct DescriptionDetail(float Start, float End, char Operator);

public enum ColumnC : byte
{
    Northern     = 0x0,
    Eastern      = 0x1,
    Southern     = 0x2,
    Western      = 0x3,
    NorthEastern = 0x4,
    SouthEastern = 0x5,
    SouthWestern = 0x6,
    NorthWestern = 0x7,
    Upper        = 0x8,
    Lower        = 0x9,
    Middle       = 0xA,

    NONE = 0xF,
}
public enum ColumnD : byte
{
    Terrace            = 0x00,
    Spur               = 0x01,
    ReEntrant          = 0x02,
    EarthBank          = 0x03,
    Quarry             = 0x04,
    EarthWall          = 0x05,
    ErosionGully       = 0x06,
    SmallErosionGully  = 0x07,
    Hill               = 0x08,
    Knoll              = 0x09,
    Saddle             = 0x0A,
    Depression         = 0x0B,
    SmallDepression    = 0x0C,
    Pit                = 0x0D,
    BrokenGround       = 0x0E,
    AntHill            = 0x0F,
    Crag               = 0x10,
    RockPillar         = 0x11,
    Cave               = 0x12,
    Boulder            = 0x13,
    BoulderField       = 0x14,
    BoulderCluster     = 0x15,
    StonyGround        = 0x16,
    BareRock           = 0x17,
    NarrowPassage      = 0x18,
    Trench             = 0x19,
    Lake               = 0x1A,
    Pond               = 0x1B,
    Waterhole          = 0x1C,
    Watercourse        = 0x1D,
    MinorWatercourse   = 0x1E,
    NarrowMarsh        = 0x1F,
    Marsh              = 0x20,
    FirmGroundInMarsh  = 0x21,
    Well               = 0x22,
    Spring             = 0x23,
    WaterTrough        = 0x24,
    OpenLand           = 0x25,
    SemiOpenLand       = 0x26,
    ForestCorner       = 0x27,
    Clearing           = 0x28,
    Thicket            = 0x29,
    LinearThicket      = 0x2A,
    VegatationBoundary = 0x2B,
    Copse              = 0x2C,
    ProminentTree      = 0x2D,
    RootStock          = 0x2E,
    Road               = 0x2F,
    Path               = 0x30,
    Ride               = 0x31,
    Bridge             = 0x32,
    PowerLine          = 0x33,
    PowerLinePylon     = 0x34,
    Tunnel             = 0x35,
    Wall               = 0x36,
    Fence              = 0x37,
    CrossingPoint      = 0x38,
    Building           = 0x39,
    PavedArea          = 0x3A,
    Ruin               = 0x3B,
    Pipeline           = 0x3C,
    Tower              = 0x3D,
    ShootingPlatform   = 0x3E,
    Cairn              = 0x3F,
    FodderRack         = 0x40,
    Platform           = 0x41,
    Statue             = 0x42,
    Canopy             = 0x43,
    Stairway           = 0x44,
    OutOfBoundsArea    = 0x45,
    SpecialItemX       = 0x46,
    SpecialItem0       = 0x47,

    NONE = 0xff,
}
public enum ColumnE : byte
{
    Terrace            = 0x00,
    Spur               = 0x01,
    ReEntrant          = 0x02,
    EarthBank          = 0x03,
    Quarry             = 0x04,
    EarthWall          = 0x05,
    ErosionGully       = 0x06,
    SmallErosionGully  = 0x07,
    Hill               = 0x08,
    Knoll              = 0x09,
    Saddle             = 0x0A,
    Depression         = 0x0B,
    SmallDepression    = 0x0C,
    Pit                = 0x0D,
    BrokenGround       = 0x0E,
    AntHill            = 0x0F,
    Crag               = 0x10,
    RockPillar         = 0x11,
    Cave               = 0x12,
    Boulder            = 0x13,
    BoulderField       = 0x14,
    BoulderCluster     = 0x15,
    StonyGround        = 0x16,
    BareRock           = 0x17,
    NarrowPassage      = 0x18,
    Trench             = 0x19,
    Lake               = 0x1A,
    Pond               = 0x1B,
    Waterhole          = 0x1C,
    Watercourse        = 0x1D,
    MinorWatercourse   = 0x1E,
    NarrowMarsh        = 0x1F,
    Marsh              = 0x20,
    FirmGroundInMarsh  = 0x21,
    Well               = 0x22,
    Spring             = 0x23,
    WaterTrough        = 0x24,
    OpenLand           = 0x25,
    SemiOpenLand       = 0x26,
    ForestCorner       = 0x27,
    Clearing           = 0x28,
    Thicket            = 0x29,
    LinearThicket      = 0x2A,
    VegatationBoundary = 0x2B,
    Copse              = 0x2C,
    ProminentTree      = 0x2D,
    RootStock          = 0x2E,
    Road               = 0x2F,
    Path               = 0x30,
    Ride               = 0x31,
    Bridge             = 0x32,
    PowerLine          = 0x33,
    PowerLinePylon     = 0x34,
    Tunnel             = 0x35,
    Wall               = 0x36,
    Fence              = 0x37,
    CrossingPoint      = 0x38,
    Building           = 0x39,
    PavedArea          = 0x3A,
    Ruin               = 0x3B,
    Pipeline           = 0x3C,
    Tower              = 0x3D,
    ShootingPlatform   = 0x3E,
    Cairn              = 0x3F,
    FodderRack         = 0x40,
    Platform           = 0x41,
    Statue             = 0x42,
    Canopy             = 0x43,
    Stairway           = 0x44,
    OutOfBoundsArea    = 0x45,
    SpecialItemX       = 0x46,
    SpecialItemO       = 0x47,
    Low                = 0x48,
    Shallow            = 0x49,
    Deep               = 0x4A,
    Overgrown          = 0x4B,
    Open               = 0x4C,
    Rocky              = 0x4D,
    Marshy             = 0x4E,
    Sandy              = 0x4F,
    NeedleLeaved       = 0x50,
    BroadLeaved        = 0x51,
    Ruined             = 0x52,

    NONE = 0xFF,
}
public enum ColumnF : byte
{
    Crossing = 0x0,
    Junction = 0x1,
    Bend     = 0x2,

    NONE = 0xF,
}
public enum ColumnG : byte
{
    NorthSide              = 0x00,
    NorthEastSide          = 0x01,
    EastSide               = 0x02,
    SouthEastSide          = 0x03,
    SouthSide              = 0x04,
    SouthWestSide          = 0x05,
    WestSide               = 0x06,
    NorthWestSide          = 0x07,
    NorthEdge              = 0x08,
    NorthEastEdge          = 0x09,
    EastEdge               = 0x0A,
    SouthEastEdge          = 0x0B,
    SouthEdge              = 0x0C,
    SouthWestEdge          = 0x0D,
    WestEdge               = 0x0E,
    NorthWestEdge          = 0x0F,
    NorthPart              = 0x10,
    NorthEastPart          = 0x11,
    EastPart               = 0x12,
    SouthEastPart          = 0x13,
    SouthPart              = 0x14,
    SouthWestPart          = 0x15,
    WestPart               = 0x16,
    NorthWestPart          = 0x17,
    NorthInsideCorner      = 0x18,
    NorthEastInsideCorner  = 0x19,
    EastInsideCorner       = 0x1A,
    SouthEastInsideCorner  = 0x1B,
    SouthInsideCorner      = 0x1C,
    SouthWestInsideCorner  = 0x1D,
    WestInsideCorner       = 0x1E,
    NorthWestInsideCorner  = 0x1F,
    NorthOutsideCorner     = 0x20,
    NorthEastOutsideCorner = 0x21,
    EastOutsideCorner      = 0x22,
    SouthEastOutsideCorner = 0x23,
    SouthOutsideCorner     = 0x24,
    SouthWestOutsideCorner = 0x25,
    WestOutsideCorner      = 0x26,
    NorthWestOutsideCorner = 0x27,
    NorthTip               = 0x28,
    NorthEastTip           = 0x29,
    EastTip                = 0x2A,
    SouthEastTip           = 0x2B,
    SouthTip               = 0x2C,
    SouthWestTip           = 0x2D,
    WestTip                = 0x2E,
    NorthWestTip           = 0x2F,
    NorthEnd               = 0x30,
    NorthEastEnd           = 0x31,
    EastEnd                = 0x32,
    SouthEastEnd           = 0x33,
    SouthEnd               = 0x34,
    SouthWestEnd           = 0x35,
    WestEnd                = 0x36,
    NorthWestEnd           = 0x37,
    UpperPart              = 0x38,
    LowerPart              = 0x39,
    Top                    = 0x3A,
    Foot                   = 0x3B,
    NorthFoot              = 0x3C,
    NorthEastFoot          = 0x3D,
    EastFoot               = 0x3E,
    SouthEastFoot          = 0x3F,
    SouthFoot              = 0x40,
    SouthWestFoot          = 0x41,
    WestFoot               = 0x42,
    NorthWestFoot          = 0x43,
    Beneath                = 0x44,
    Between                = 0x45,

    NONE = 0xFF,
}
public enum ColumnH : byte
{
    FirstAidPost     = 0x0,
    RefreshmentPoint = 0x1,
    MannedControl    = 0x2,

    NONE = 0xF,
}