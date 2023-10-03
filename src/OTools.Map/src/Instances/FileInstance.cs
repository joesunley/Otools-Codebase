namespace OTools.Maps;

public sealed class FileInstance : Instance<FileSymbol>
{
    public vec2 Centre { get; set; }

    /// <warning>Measured in Degrees</warning>
    public float Rotation { get; set; }
    public vec2 Scaling { get; set; }

    public FileInstance(int layer, FileSymbol symbol, vec2 centre, float rotation, vec2 scaling)
        : base(layer, symbol)
    {
        Centre = centre;
        Rotation = rotation;
        Scaling = scaling;
    }

    public FileInstance(Guid id, int layer, FileSymbol symbol, vec2 centre, float rotation, vec2 scaling)
        : base(id, layer, symbol)
    {
        Centre = centre;
        Rotation = rotation;
        Scaling = scaling;
    }
}