using OTools.Common;

namespace OTools.Maps;


public sealed class ColourStore : Store<Colour>
{
    public ColourStore() { }
    public ColourStore(IEnumerable<Colour> items) : base(items) { }

    public Colour this[string name]
    {
        get => _items.First(s => s.Name == name);
        set => _items[_items.IndexOf(this.First(x => x.Name == name))] = value;
    }

    public void UpdatePrecendences(byte offset)
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i].Precedence = (ushort)(i + offset);
    }
}

public sealed class SymbolStore : Store<Symbol>
{
    public SymbolStore() { }
    public SymbolStore(IEnumerable<Symbol> items) : base(items) { }

    public Symbol this[string name]
    {
        get => _items.First(s => s.Name == name);
        set => _items[_items.IndexOf(this.First(x => x.Name == name))] = value;
    }
}

public sealed class InstanceStore : Store<Instance>
{
    public Dictionary<string, int> Layers { get; set; }

    public InstanceStore()
    {
        Layers = new() { { "Main Layer", 0 } };
    }

    public InstanceStore(IEnumerable<Instance> items) : base(items)
    {
        Layers = new() { { "Main Layer", 0 } };
    }

    public void SwapLayers(int layer1, int layer2)
    {
        int maxLayer = _items.Select(x => x.Layer).Max();

        if (layer1 > maxLayer || layer2 > maxLayer ||
            layer1 < 0 || layer2 < 0)
            return;

        IEnumerable<Instance> layer1Objects = _items.Where(x => x.Layer == layer1);
        IEnumerable<Instance> layer2Objects = _items.Where(x => x.Layer == layer2);

        foreach (Instance item in layer1Objects)
            item.Layer = layer2;

        foreach (Instance item in layer2Objects)
            item.Layer = layer1;
    }

    public void SetLayerOpacity(int layer, float opacity)
    {
        foreach (Instance item in _items.Where(x => x.Layer == layer))
            item.Opacity = opacity;
    }

    public void SetLayerOpacity(string layer, float opacity)
        => SetLayerOpacity(Layers[layer], opacity);
}

public class TemplateStore : Store<Template>
{
    public TemplateStore()  { }
    
    public TemplateStore(IEnumerable<Template> items) : base(items) { }
}