namespace OTools.Maps;

public sealed class PointSymbol : Symbol
{
    public List<MapObject> MapObjects { get; set; }

    public bool IsRotatable { get; set; }

    public PointSymbol(string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, IEnumerable<MapObject> mapObjects, bool isRotatable)
        : base(name, description, number, isUncrossable, isHelperSymbol)
    {
        MapObjects = new(mapObjects);
        IsRotatable = isRotatable;
    }

    public PointSymbol(Guid id, string name, string description, SymbolNumber number, bool isUncrossable, bool isHelperSymbol, IEnumerable<MapObject> mapObjects, bool isRotatable)
        : base(id, name, description, number, isUncrossable, isHelperSymbol)
    {
        MapObjects = new(mapObjects);
        IsRotatable = isRotatable;
    }
}