using OneOf;
using OTools.Maps;
using Svg;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace OTools.ObjectRenderer2D;

public static class SvgConverter
{
    public static IShape ConvertElement(SvgElement el)
    {
        return el switch
        {
            SvgCircle c => ConvertCircle(c),
            //SvgEllipse e => ConvertEllipse(e),
            //SvgLine l => ConvertLine(l),
            //SvgPath p => ConvertPath(p),
            //SvgPolygon p => ConvertPolygon(p),
            //SvgPolyline p => ConvertPolyline(p),
            //SvgRectangle r => ConvertRectangle(r),
            _ => new Rectangle(),
        };
    }

    public static IShape ConvertCircle(SvgCircle inp)
    {
        Circle c = new()
        {
            Diameter = inp.Radius * 2,

            TopLeft = (inp.CenterX, inp.CenterY),

            Fill = ConvertPaintServer(inp.Fill).AsT0,

            BorderColour = ConvertPaintServer(inp.Stroke).AsT0,
            BorderWidth = inp.StrokeWidth,

            DashArray = ConvertDashArray(inp.StrokeDashArray),

            Opacity = inp.Opacity
        };

        return c;
    }

    private static IEnumerable<double> ConvertDashArray(SvgUnitCollection strokeDashArray)
    {
        foreach (var item in strokeDashArray)
            yield return item.Value;
    }

    private static Colour ConvertColour(SvgPaintServer stroke)
    {
        var col = stroke as SvgColourServer ?? throw new ArgumentException("Stroke was not a colour");

        return new RgbColour("", col.Colour.R, col.Colour.G, col.Colour.B, col.Colour.A);
    }

    private static OneOf<Colour, IFill> ConvertPaintServer(SvgPaintServer server)
    {
        switch (server)
        {
            case SvgColourServer col:
                return new RgbColour("", col.Colour.R, col.Colour.G, col.Colour.B, col.Colour.A);
            case SvgPatternServer pattern:
                return Colour.Transparent;
            default: throw new NotImplementedException();
        }
    }
}