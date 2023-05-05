namespace OTools.Maps;

public class MapInfo
{
    public LayerInfo LayerInfo { get; set; }
    public string FilePath { get; set; }

    public MapInfo(LayerInfo layerInfo, string filePath)
    {
        LayerInfo = layerInfo;
        FilePath = filePath;
    }
}

public class LayerInfo : List<(string name, bool visible)>
{
    public LayerInfo()
    {
        Add(("Main", true));
    }

    public void Swap(int index, sbyte direction)
    {
        Assert(direction is 1 or -1);

        // Some fancy swap that Rider recommended
        (this[index + direction], this[index]) = (this[index], this[index + direction]);
    }
}