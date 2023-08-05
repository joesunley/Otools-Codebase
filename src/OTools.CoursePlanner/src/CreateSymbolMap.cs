using OTools.Common;
using OTools.Maps;

namespace OTools.CoursePlanner;

public static class CreateSymbolMap
{
    public static Map DefaultISOM()
    {
        Map map = new();

        Colour purple = new CmykColour("Upper Purple", .35f, .85f, 0, 0);
        Colour white = new CmykColour("White", 0, 0, 0, 0);
        map.Colours.Add(purple);
        map.Colours.Add(white);

        IEnumerable<vec2> triPoints = MathUtils.CreateEquilateralTriangle(6);
        PathCollection pC = new() { new LinearPath(triPoints) };
        LineObject sObj = new(pC, .35f, purple, true);
        PointSymbol start = new("Start", string.Empty, (701, 0, 0), false, false, sObj.Yield(), true);

        LineObject miObj = new(new() { new LinearPath(new vec2[] { (0, 0), (2.5, 0) }) }, .6f, purple, false);
        PointSymbol mapIssue = new("Map Issue Point", string.Empty, (702, 0, 0), false, false, miObj.Yield(), true);

        PointObject cObj = new(Colour.Transparent, purple, 2.15f, .35f); // Not 100% sure about 4.3f
        PointSymbol control = new("Control", string.Empty, (703, 0, 0), false, false, cObj.Yield(), false);

        Font font = new("Arial", purple, 4f, 1f, 1f, 1f, FontStyle.None);
        TextSymbol controlNum = new("Control Number", string.Empty, (704, 0, 0), false, false, font, false, Colour.Transparent, 0f, white, 0f);

        LineSymbol courseLine = new("Course Line", string.Empty, (705, 0, 0), false, false, purple, .35f, DashStyle.None, MidStyle.None, LineStyle.Default, BorderStyle.None);

        PointObject fObjInner = new(Colour.Transparent, purple, 1.65f, .35f); // Not 100% sure about 3.3f
        PointObject fObjOuter = new(Colour.Transparent, purple, 2.65f, .35f); // Not 100% sure about 5.3f
        PointSymbol finish = new("Finish", string.Empty, (706, 0, 0), false, false, new[] { fObjInner, fObjOuter }, false);

        LineSymbol markedRoute = new("Marked Route", string.Empty, (707, 0, 0), false, false, purple, .35f, new(2f, .5f), MidStyle.None, LineStyle.Default, BorderStyle.None);

        LineSymbol oobBoundary = new("Out-of-Bounds Boundary", string.Empty, (708, 0, 0), true, false, purple, .7f, DashStyle.None, MidStyle.None, LineStyle.Default, BorderStyle.None);

        PatternFill fill1 = new(.2f, .8f, purple, Colour.Transparent, 45);
        PatternFill fill2 = new(.2f, .8f, purple, Colour.Transparent, 180 - 45);
        CombinedFill fill = new(new[] { fill1, fill2 });
        AreaSymbol oobArea = new("Out-of-Bounds Area", string.Empty, (709, 0, 0), true, false, fill, Colour.Transparent, 0f, DashStyle.None, MidStyle.None, LineStyle.Default, BorderStyle.None, false); // Maybe change to true for last

        LineSymbol oobAreaLine = new("Out-of-Bounds Area Border", string.Empty, (709, 1, 0), true, false, purple, .25f, DashStyle.None, MidStyle.None, LineStyle.Default, BorderStyle.None);
        LineSymbol oobAreaLineDashed = new("Out-of-Bounds Area Dashed Border", string.Empty, (709, 2, 0), true, false, purple, .25f, new(3f, .5f), MidStyle.None, LineStyle.Default, BorderStyle.None);

        map.Symbols.Add(start);
        map.Symbols.Add(mapIssue);
        map.Symbols.Add(control);
        map.Symbols.Add(controlNum);
        map.Symbols.Add(courseLine);
        map.Symbols.Add(finish);
        map.Symbols.Add(markedRoute);
        map.Symbols.Add(oobBoundary);
        map.Symbols.Add(oobArea);
        map.Symbols.Add(oobAreaLine);
        map.Symbols.Add(oobAreaLineDashed);

        return map;
    }

    public static IEnumerable<Symbol> DefaultISSprOM()
    {
        throw new NotImplementedException();
    }
}