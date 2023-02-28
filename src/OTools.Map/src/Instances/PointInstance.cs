using OTools.Maps;

namespace OTools.Maps;

public sealed class PointInstance : Instance<PointSymbol>
{
    public vec2 Centre { get; set; }

    /// <warning>Measured in Degrees</warning>
    public float Rotation { get; set; }

    public PointInstance(int layer, PointSymbol symbol, vec2 centre, float rotation)
        : base(layer, symbol)
    {
        Centre = centre;
        Rotation = rotation;
    }

    public PointInstance(Guid id, int layer, PointSymbol symbol, vec2 centre, float rotation)
        : base(id, layer, symbol)
    {
        Centre = centre;
        Rotation = rotation;
    }

    public override vec4 GetBoundingBox()
    {
        vec2 topLeft = vec2.MaxValue,
             bottomRight = vec2.MinValue;

        foreach (MapObject mapObject in Symbol.MapObjects)
        {
            vec4 bBox = mapObject.GetBoundingBox() + (Centre, Centre);

            topLeft = vec2.Min(topLeft, bBox.XY);
            bottomRight = vec2.Max(bottomRight, bBox.ZW);
        }

        return (topLeft, bottomRight);
    }
}