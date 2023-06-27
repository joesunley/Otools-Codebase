using OTools.ObjectRenderer2D;

namespace OTools.Routechoice;

public static class Rotate
{
	public static (vec4 leg, vec3 rotVals) Do(vec4 leg, float rotOffset = 0f)
	{
		vec2 midpoint = (leg.XY + leg.ZW) / 2;
		
		float angle = MathF.Atan2(leg.Y - leg.W, leg.Z - leg.X);
		
		vec2[] points =  { leg.XY, leg.ZW };

		vec2[] rotated = PolygonTools.Rotate(points, angle, midpoint).ToArray();
		rotated = PolygonTools.Rotate(rotated, rotOffset, midpoint).ToArray();
		
		vec4 legOut = (rotated[0], rotated[1]);

		vec3 rotValsOut = new(midpoint, angle);

		return (legOut, rotValsOut);
	}
}