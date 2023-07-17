using OTools.Maps;
using System.Diagnostics;

namespace OTools.AvaCommon;

public static class BoundingBox
{
    public static vec4 OfInstance(Instance inst)
    {
        switch (inst)
        {
            case PointInstance p: return OfSymbol(p.Symbol) + (p.Centre, p.Centre);
            case LineInstance l: return OfSegments(l.Segments);
            case AreaInstance a: return OfSegments(a.Segments);
            case TextInstance t: return vec4.Zero; // TODO: Implement
            default: throw new InvalidOperationException();
        }
    }

    public static vec4 OfSymbol(Symbol s)
    {
        switch (s)
        {
            case PointSymbol p: {
                vec2 topLeft = vec2.MaxValue,
                    bottomRight = vec2.MinValue;

                foreach (MapObject obj in p.MapObjects)
                {
                    vec4 objBBox = OfMapObject(obj);
                    topLeft = vec2.Min(topLeft, objBBox.XY);
                    bottomRight = vec2.Max(bottomRight, objBBox.ZW);
                }

                Debug.WriteLine($"BOx: {new vec4(topLeft, bottomRight)}");

                return (topLeft, bottomRight);
            }
            default: throw new InvalidOperationException($"Cannot Calculate Bounding Box of {nameof(s)}");
        }
    }

    public static vec4 OfMapObject(MapObject obj)
    {
        switch (obj)
        {
            case PointObject p: {
                float radius = p.InnerRadius + p.OuterRadius;

                vec2 topLeft = (-radius, -radius);
                vec2 bottomRight = (radius, radius);

                return (topLeft, bottomRight);
            }
            case LineObject l: return OfSegments(l.Segments) + ((-l.Width, -l.Width), (l.Width, l.Width));
            case AreaObject a: return OfSegments(a.Segments) + ((-a.BorderWidth, -a.BorderWidth), (a.BorderWidth, a.BorderWidth));
            case TextObject t: return vec4.Zero; // TODO: Implement
            default: throw new InvalidOperationException();
        }
    }

    private static vec4 OfSegments(PathCollection segments)
    {
        vec2 topLeft = vec2.MaxValue,
            bottomRight = vec2.MinValue;

        IList<vec2> poly = segments.LinearApproximation();

        foreach (vec2 point in poly)
        {
            topLeft = vec2.Min(topLeft, point);
            bottomRight = vec2.Max(bottomRight, point);
        }

        return (topLeft, bottomRight);
    }   
}