namespace OTools.Maps;

public class MapInfo
{
    public LayerInfo LayerInfo { get; set; }
    public string FilePath { get; set; }
    public ColourSpace ColourSpace { get; set; }
    public ColourLUT ColourLUT { get; set; }
    
    public MapInfo()
    {
        LayerInfo = new LayerInfo();
        FilePath = "";
        ColourSpace = ColourSpace.RGB;
        ColourLUT = new();
    }

    public MapInfo(LayerInfo layerInfo, string filePath, ColourSpace colourSpace, ColourLUT colourLut)
    {
        LayerInfo = layerInfo;
        FilePath = filePath;
        ColourSpace = colourSpace;
        ColourLUT = colourLut;
    }
}

public class LayerInfo : List<(string name, float opacity)>
{
    public LayerInfo()
    {
        Add(("Main", 1f));
    }
    public LayerInfo(IEnumerable<(string, float)> layers)
        : base(layers)
    {
        if (layers == null)
            Add(("Main", 1f));
    }

    public void Swap(int index, sbyte direction)
    {
        Assert(direction is 1 or -1);

        // Some fancy swap that Rider recommended
        (this[index + direction], this[index]) = (this[index], this[index + direction]);
    }
}