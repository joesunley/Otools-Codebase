using System.Collections;
using System.ComponentModel;
using OTools.Maps;

namespace OTools.Maps;

public abstract class BaseStore<T> : IList<T> where T : IStorable
{
    protected readonly BindingList<T> _items;

    public event Action<ListChangedEventArgs>? Changed;

    protected BaseStore()
    {
        _items = new();
        _items.ListChanged += (_, e) => Changed?.Invoke(e);
    }

    protected BaseStore(IEnumerable<T> items)
    {
        _items = new(items.ToList());
        _items.ListChanged += (_, e) => Changed?.Invoke(e);
    }

    public int IndexOf(T item) => _items.IndexOf(item);
    public void Insert(int index, T item) => _items.Insert(index, item);
    public void RemoveAt(int index) => _items.RemoveAt(index);
    public void Add(T obj) => _items.Add(obj);
    public void Clear() => _items.Clear();
    public bool Contains(T item) => _items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public bool Remove(T item) => _items.Remove(item);
    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public T this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public T this[Guid id]
    {
        get => _items.First(s => s.Id == id);
        set => _items[_items.IndexOf(_items.First(s => s.Id == id))] = value;
    }

    public bool Contains(Guid id) => _items.Any(s => s.Id == id);
}

public interface IStorable
{
    Guid Id { get; }
}

public sealed class ColourStore : BaseStore<Colour>
{
    public ColourStore() { }
    public ColourStore(IEnumerable<Colour> items) : base(items) { }

    public Colour this[string name]
    {
        get => _items.First(s => s.Name == name);
        set => _items[_items.IndexOf(this.First(x => x.Name == name))] = value;
    }

    public int GetZIndex(Colour col)
    {
        if (col.Name == "Transparent") return 0;

        int currLoc = _items.IndexOf(col);
        if (currLoc == -1) throw new ArgumentOutOfRangeException(nameof(col), "Colour not found");

        int max = _items.Count - 1;

        return max - currLoc;
    }
}

public sealed class SymbolStore : BaseStore<Symbol>
{
    public SymbolStore() { }
    public SymbolStore(IEnumerable<Symbol> items) : base(items) { }

    public Symbol this[string name]
    {
        get => _items.First(s => s.Name == name);
        set => _items[_items.IndexOf(this.First(x => x.Name == name))] = value;
    }
}

public sealed class InstanceStore : BaseStore<Instance>
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