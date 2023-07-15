using Sunley.Mathematics;

namespace OTools.Common;

public static class MathUtils
{
    public static IEnumerable<vec2> CreateEquilateralTriangle(float size, vec2? centreParam = null)
    {
        vec2 centre = centreParam ?? vec2.Zero;
        float sizeSquared = (float)Math.Pow(size, 2);

        float y = sizeSquared / (2 * size);
        float x = (float)Math.Sqrt(sizeSquared - Math.Pow(y, 2));

        vec2 a = (0, 0), b = (size, 0), c = (y, -x);

        vec2 off = (y, -x / 2.5);



        return new[] { a, b, c }.Select(x => x - off + centre);
    }
}