using Svg.Skia;

namespace OTools.Maps;

public abstract class FileSymbol : Symbol
{
    public Uri FilePath { get; set; }

    protected FileSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Uri filePath)
        : base(name, description, number, isUncrossable, isHelperSymbol)
    {
        FilePath = filePath;
    }
    protected FileSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Uri filePath)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)

    {
        FilePath = filePath;
    }
}

public sealed class BitmapSymbol : FileSymbol
{
    public float Resolution { get; set; }

    public BitmapSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Uri filePath, float resolution)
        : base(name, description, number, isUncrossable, isHelperSymbol, filePath)
    {
        Resolution = resolution;
    }
    public BitmapSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Uri filePath, float resolution)
        : base(id, name, description, number, isUncrossable, isHelperSymbol, filePath)

    {
        Resolution = resolution;
    }
}

public sealed class VectorSymbol : FileSymbol
{
    public vec2 Scale { get; set; }

    public VectorSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Uri filePath, vec2 scale)
        : base(name, description, number, isUncrossable, isHelperSymbol, filePath)
    {
        Scale = scale;
    }
    public VectorSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, Uri filePath, vec2 scale)
        : base(id, name, description, number, isUncrossable, isHelperSymbol, filePath)

    {
        Scale = scale;
    }
}