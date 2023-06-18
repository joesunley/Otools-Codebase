using OTools.Common;
using OTools.Maps;

namespace OTools.Routechoice;

public static class MapCreation
{
    public static Map Create()
    {
        Colour purple = new("Purple", (25, 85, 0, 0));

        Map map = new();

        map.Colours.Add(purple);

        vec2[] vec2s = { (-3.5, -2.021), (0, 4.041), (3.5, -2.021), (-3.5, -2.021) };
        LineObject tri = new(new(vec2s), .35f, purple);
        PointSymbol startTriangle = new("Start", "", new(701, 0, 0), false, false, tri.Yield(), true);

        LineSymbol courseLine = new("Line", "", new(704, 0, 0), false, false, purple, .35f, DashStyle.None, MidStyle.None, LineStyle.Default, BorderStyle.None);

        PointObject circ = new(Colour.Transparent, purple, 2.825f, .35f);
        PointSymbol controlPoint = new("Control", "", new(702, 0, 0), false, false, circ.Yield(), false);

        PointObject circ2 = new(Colour.Transparent, purple, 2.325f, .35f);
        PointObject circ3 = new(Colour.Transparent, purple, 3.325f, .35f);
        PointSymbol finishCircle = new("Finish", "", new(706, 0, 0), false, false, circ2.Yield().Concat(circ3.Yield()), false);

        map.Symbols.Add(startTriangle);
        map.Symbols.Add(courseLine);
        map.Symbols.Add(controlPoint);
        map.Symbols.Add(finishCircle);

        return map;
    }
}