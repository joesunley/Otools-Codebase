using OTools.Maps;

namespace OTools.Symbols;

public sealed class SymbolSet
{
    public ColourStore Colours { get; set; }
    public SpotColourStore SpotColours { get; set; }

    public Dictionary<string, SymbolStore> Symbols { get; set; }

    public SymbolSet()
    {
        Colours = new();
        SpotColours = new();
        Symbols = new();
    }

    public SymbolSet(ColourStore colours, SpotColourStore spotColours, Dictionary<string, SymbolStore> symbols)
    {
        Colours = colours;
        SpotColours = spotColours;
        Symbols = symbols;
    }

    public Map CreateMap(string symbols)
    {
        SymbolStore syms = Symbols[symbols];

        return new(Colours, SpotColours, syms, new(), "");
    }
}