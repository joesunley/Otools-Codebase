namespace OTools.Maps;

public class PointInstance : Instance<PointSymbol>
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
}