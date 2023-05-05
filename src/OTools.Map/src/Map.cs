global using static System.Diagnostics.Debug;

namespace OTools.Maps;

public sealed class Map
{
    public string Title { get; set; }

    public MapInfo MapInfo { get; set; }

    public ColourStore Colours { get; set; }
    public SymbolStore Symbols { get; set; }
    public InstanceStore Instances { get; set; }

    public Map(string title = "")
    {
        Title = title;

        Colours = new();
        Symbols = new();
        Instances = new();
    }

    public Map(ColourStore colours, SymbolStore symbols, InstanceStore instances, string title = "")
    {
        Title = title;

        Colours = colours;
        Symbols = symbols;
        Instances = instances;
    }
}