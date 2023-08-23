using OTools.Maps;

namespace OTools.Symbols;

public static class OfficialSymbols
{
    public static SymbolSet Create()
    {
        SpotColourStore spotCols = new();

        CmykColour black = new("Spot: Black", 0, 0, 0, 1);
        CmykColour blue = new("Spot: Blue", 1, 0, 0, 0);
        CmykColour yellow = new("Spot: Yellow", 0, .27f, .79f, 0);
        CmykColour green = new("Spot: Green", .76f, 0, .91f, 0);
        CmykColour darkGreen = new("Spot: Dark Green", 1, 0, .8f, .3f);
        CmykColour brown = new("Spot: Brown", 0, .56f, 1, .18f);
        CmykColour purple = new("Spot: Purple", .35f, .85f, 0, 0);
        CmykColour white = new("Spot: White", 0, 0, 0, 0);

        SpotCol blackS = new("Black", black);
        SpotCol blueS = new("Blue", blue);
        SpotCol yellowS = new("Yellow", yellow);
        SpotCol greenS = new("Green", green);
        SpotCol darkGreenS = new("Dark Green", darkGreen);
        SpotCol brownS = new("Brown", brown);
        SpotCol purpleS = new("Purple", purple);
        SpotCol whiteS = new("White", white); // Easier Assigning


        SpotColour upperPurple = ColourBuilder.SpotColour("Upper purple for course overprint")
            .AddColour(purpleS, 1f);

        SpotColour whiteForOver = ColourBuilder.SpotColour("White for course overprint")
            .AddColour(whiteS, 1f);

        SpotColour purple50 = ColourBuilder.SpotColour("Purple 50% area symbol")
            .AddColour(purpleS, .5f);

        CmykColour greenSki = new CmykColour("Green for Ski-O", .91f, 0, .83f, 0);

        SpotColour whiteRail = ColourBuilder.SpotColour("White for railroad")
            .AddColour(whiteS, 1f);

        SpotColour black100 = ColourBuilder.SpotColour("Black 100%")
            .AddColour(blackS, 1f);

        SpotColour blue100Point = ColourBuilder.SpotColour("Blue 100% point symbols")
            .AddColour(blueS, 1f);

        SpotColour brown100Point = ColourBuilder.SpotColour("Brown 100% point symbols")
            .AddColour(brownS, 1f);

        SpotColour green100Point = ColourBuilder.SpotColour("Green 100% point symbols")
            .AddColour(greenS, 1f);

        SpotColour blue100Line = ColourBuilder.SpotColour("Blue 100% line symbols")
            .AddColour(blueS, 1f);

        SpotColour darkGreenLineISOM = ColourBuilder.SpotColour("Dark green line symbols")
            .AddColour(darkGreenS, 1f);

        SpotColour brown100Line = ColourBuilder.SpotColour("Brown 100% line symbols")
            .AddColour(brownS, 1f);

        SpotColour lowerPurple = ColourBuilder.SpotColour("Lower purple for course overprint")
            .AddColour(purpleS, 1f);

        SpotColour darkGreenLine = ColourBuilder.SpotColour("Dark green line symbols")
            .AddColour(darkGreenS, 1f);

        SpotColour blue100Both = ColourBuilder.SpotColour("Blue 100% point and line symbols")
            .AddColour(blueS, 1f);

        SpotColour brown100 = ColourBuilder.SpotColour("Brown 100%")
            .AddColour(brownS, 1f);

        SpotColour whiteStripes = ColourBuilder.SpotColour("White stripes for area passable at two levels")
            .AddColour(whiteS, 1f);

        SpotColour brown50Road = ColourBuilder.SpotColour("Brown 50% for road infill")
            .AddColour(brownS, .5f);
        
        SpotColour brown30Road = ColourBuilder.SpotColour("Brown 30% for road infill")
            .AddColour(brownS, .3f);

        SpotColour black100Road = ColourBuilder.SpotColour("Black 100% for road outline")
            .AddColour(blackS, 1f);

        SpotColour black60Buildings = ColourBuilder.SpotColour("Black 60% for buildings")
            .AddColour(blackS, .6f);

        SpotColour black50tramway = ColourBuilder.SpotColour("Black 50% for large buildings and tramway")
            .AddColour(blackS, .5f);

        SpotColour black20canopy = ColourBuilder.SpotColour("Black 20% for canopy")
            .AddColour(blackS, .2f);

        SpotColour blue100Area = ColourBuilder.SpotColour("Blue 100% area symbols")
            .AddColour(blueS, 1f);

        SpotColour blue70Area = ColourBuilder.SpotColour("Blue 70% area symbols")
            .AddColour(blueS, .7f);
        
        SpotColour blue50Area = ColourBuilder.SpotColour("Blue 50% area symbols")
            .AddColour(blueS, .5f);
        
        SpotColour blue30Area = ColourBuilder.SpotColour("Blue 30% area symbols")
            .AddColour(blueS, .3f);

        SpotColour whiteOver = ColourBuilder.SpotColour("White over green and brown")
            .AddColour(whiteS, 1f);

        SpotColour brown50Paved = ColourBuilder.SpotColour("Brown 50% for paved area")
            .AddColour(brownS, .5f);

        SpotColour brown30Paved = ColourBuilder.SpotColour("Brown 30% for paved area")
            .AddColour(brownS, .3f);

        SpotColour yellowGreen = ColourBuilder.SpotColour("Yellow 100% + Green 50%")
            .AddColour(yellowS, 1f)
            .AddColour(greenS, .5f);

        SpotColour darkGreenArea = ColourBuilder.SpotColour("Dark green area symbols")
            .AddColour(darkGreenS, 1f);

        SpotColour green100Area = ColourBuilder.SpotColour("Green 100% area symbols")
            .AddColour(greenS, 1f);

        SpotColour green60Area = ColourBuilder.SpotColour("Green 60% area symbols")
            .AddColour(greenS, .6f);

        SpotColour green30Area = ColourBuilder.SpotColour("Green 30% area symbols")
            .AddColour(greenS, .3f);

        SpotColour black30Area = ColourBuilder.SpotColour("Black 30% area symbols")
            .AddColour(blackS, .3f);

        SpotColour whiteYellow = ColourBuilder.SpotColour("White over Yellow")
            .AddColour(whiteS, 1f);

        SpotColour blackCultivated = ColourBuilder.SpotColour("Black for cultivated land and sandy ground")
            .AddColour(blackS, 1f);

        CmykColour orangeMtb = new("Orange for open land permitted to ride", 0, .6f, 1, 0);

        SpotColour yellow100 = ColourBuilder.SpotColour("Yellow 100% area symbols")
            .AddColour(yellowS, 1f);

        SpotColour yellow75 = ColourBuilder.SpotColour("Yellow 75% area symbols")
            .AddColour(yellowS, .75f);

        SpotColour yellow50 = ColourBuilder.SpotColour("Yellow 50% area symbols")
            .AddColour(yellowS, .5f);

        LineSymbol contour = SymbolBuilder.Start("Contour", 101)
            .SetDescription("A line joining points of equal heights")
            .SetColour(brown100Line)
            .SetWidth(.21f)
            .CreateLine();

        LineSymbol indexContour = SymbolBuilder.Start("Index Contour", 102)
            .SetDescription("Every fifth contour shall be drawn with a thicker line. This is an aid to the quick assessment of height difference and the overall shape of the terrain.")
            .SetColour(brown100Line)
            .SetWidth(.3f)
            .CreateLine();

        LineSymbol formLine = SymbolBuilder.Start("Form Line", 103)
            .SetDescription("An intermediate contour line. Form lines are used where more information can be given about the shape of the ground")
            .SetColour(brown100Line)
            .SetWidth(.15f)
            .SetDash(3f, .3f)
            .CreateLine();

        LineSymbol earthBank = SymbolBuilder.Start("Earth Bank", 104)
            .SetDescription("An abrupt change in ground level which can be clearly distinguished from its surrounding.")
            .SetColour(brown100Line)
            .SetWidth(.37f)
            .CreateLine();

        LineSymbol smallEarthWall = SymbolBuilder.Start("Small Earth Wall", 105)
            .SetDescription("A distinct earth wall, usually man-made")
            .SetColour(brown100Line)
            .SetWidth(.21f)
            .SetMid(3.75f, true, 0f, 0f)
            .AddMidObj(new PointObject(brown100Line, Colour.Transparent, .3f, 0f))
            .CreateLine();

        LineSymbol erosionGully = SymbolBuilder.Start("Erosion Gully or Trench", 107)
            .SetDescription("An erosion gully or trench which is too small to be represented with 104, 101, 102 or 103")
            .SetColour(brown100Line)
            .SetWidth(.37f)
            .CreateLine();

        LineSymbol smallErosionGully = SymbolBuilder.Start("Small Erosion Gully", 108)
            .SetDescription("A small erosion gully or trench. Contour lines should be broken around this symbol")
            .SetMid(.6f, true, 0f, 0f)
            .AddMidObj(new PointObject(brown100Line, Colour.Transparent, .185f, 0f))
            .CreateLine();

        PointSymbol smallKnoll = SymbolBuilder.Start("Small Knoll", 109)
            .SetDescription("A small obvious mound or rocky knoll which cannot be drawn to scale with 101, 102 or 103")
            .AddMapObj(new PointObject(brown100Line, Colour.Transparent, .375f, 0f))
            .CreatePoint();

        PointSymbol smallElongatedKnoll = SymbolBuilder.Start("Small Elongated Knoll", 110)
            .SetDescription("A small obvious elongated mound or rocky knoll which cannot be drawn to scale with 101, 102 or 103")
            .SetBools(false, false, true)
            .CreatePoint();

        PointSymbol smallDepression = SymbolBuilder.Start("Small Depression", 111)
            .SetDescription("A small shallow natural depression or hollow which cannot be represented by 101, 103")
            .CreatePoint();

        PointSymbol pitOrHole = SymbolBuilder.Start("Pit or Hole", 112)
            .SetDescription("A pit or hole with distinct steep side which cannot be represented to scale with 104")
            .CreatePoint();

        AreaSymbol brokenGround = SymbolBuilder.Start("Broken Ground", 113)
            .SetDescription("An area of pits and/or knolls, which is too complex to be represented in detail")
            .CreateArea();

        PointSymbol prominentLandformFeature = SymbolBuilder.Start("Prominent Landform  Feature", 115)
            .SetDescription("A small landform feature which is significant or prominent")
            .CreatePoint();


        LineSymbol uncrossableCliff = SymbolBuilder.Start("Uncrossable Cliff", 201)
            .SetDescription("An uncrossable cliff, quarry or earth bank. For vertical rock faces the tags mayy be omitted if space is short")
            .SetColour(black100)
            .SetWidth(.5f)
            .CreateLine();

        LineSymbol passableRockFace = SymbolBuilder.Start("Passable rock face", 202)
            .SetDescription("An passable cliff or quarry. For vertical rock faces the tags mayy be omitted if space is short")
            .SetColour(black100)
            .SetWidth(.3f)
            .CreateLine();

        PointSymbol rockPitOrCave = SymbolBuilder.Start("Rocky Pit or Cave", 203)
            .SetDescription("A rocky pit, hole, cave or mineshaft which may constitute a danger to the competitor")
            .SetBools(false, false, true)
            .CreatePoint();

        PointSymbol boulder = SymbolBuilder.Start("Boulder", 204)
            .SetDescription("A small distinct boulder")
            .AddMapObj(new PointObject(black100, Colour.Transparent, .3f, 0f))
            .CreatePoint();

        PointSymbol largeBoulder = 
    }
}

file static class ColourBuilder
{
    public static SpotColour SpotColour(string name)
    {
        return new(name, new());
    }

    public static SpotColour AddColour(this SpotColour spotCol, SpotCol col, float factor)
    {
        spotCol.SpotColours.Add(col, factor);
        return spotCol;
    }
}

file class SymbolBuilder
{
    private string name, desc;
    private SymbolNumber num;
    private bool uncr, help, rot;
    List<MapObject> mapObj = new();

    public static SymbolBuilder Start(string name, SymbolNumber num)
    {
        SymbolBuilder b = new();

        b.name = name;
        b.num = num;

        return b;
    }

    public SymbolBuilder SetDescription(string desc)
    {
        this.desc = desc;
        return this;
    }

    public SymbolBuilder SetBools(bool uncr, bool help, bool rot)
    {
        this.uncr = uncr;
        this.help = help;
        this.rot = rot;

        return this;
    }

    public SymbolBuilder AddMapObj(MapObject obj)
    {
        mapObj.Add(obj);
        return this;
    }

    public PointSymbol CreatePoint()
    {
        return new(name, desc, num, uncr, help, mapObj, rot);
    }


    private Colour col = Colour.Transparent;
    private float width = 0f;
    private DashStyle dash = DashStyle.None;
    private MidStyle mid = MidStyle.None;
    private LineStyle line = LineStyle.Default;
    private BorderStyle border = BorderStyle.None;

    public SymbolBuilder SetColour(Colour col)
    {
        this.col = col;
        return this;
    }

    public SymbolBuilder SetWidth(float wid)
    {
        this.width = wid;
        return this;
    }

    public SymbolBuilder SetDash(float dashLen, float gapLen, int groupSize = 0, float grGapLen = 0f)
    {
        this.dash = new(dashLen, gapLen, groupSize, grGapLen);
        return this;
    }

    public SymbolBuilder SetMid(float gapLen, bool require, float initOff, float endOff)
    {
        this.mid = new(Enumerable.Empty<MapObject>(), gapLen, require, initOff, endOff);
        return this;
    }

    public SymbolBuilder AddMidObj(MapObject obj)
    {
        this.mid.MapObjects.Add(obj);
        return this;
    }

    public SymbolBuilder SetLineStyle(LineStyle.JoinStyle join, LineStyle.CapStyle cap)
    {
        this.line = new(join, cap);
        return this;
    }

    public SymbolBuilder SetBorder(Colour col, float wid, float off, DashStyle? dash = null, MidStyle? mid = null)
    {
        this.border = new(col, wid, off, dash, mid);
        return this;
    }

    public LineSymbol CreateLine()
    {
        return new(name, desc, num, uncr, help, col, width, dash, mid, line, border);
    }

    private IFill fill;
    private bool rotPat;

    public SymbolBuilder SetFill(IFill fill)
    {
        this.fill = fill;
        return this;
    }

    public SymbolBuilder SetRotatablePattern()
    {
        this.rotPat = true;
        return this;
    }

    public AreaSymbol CreateArea()
    {
        return new(name, desc, num, uncr, help, fill, col, width, dash, mid, line, border, rotPat);
    }
}