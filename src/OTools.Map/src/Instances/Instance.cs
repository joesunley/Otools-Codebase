using OTools.Common;

namespace OTools.Maps;

[DebuggerDisplay("{Symbol.Name}, {Id}")]
public abstract class Instance<T> : Instance where T : Symbol
{
    public Guid Id { get; init; }
    public int Layer { get; set; }
    public float Opacity { get; set; }
    public T Symbol { get; set; }
    Symbol Instance.Symbol
    {
        get => Symbol;
        set => Symbol = (T)value;
    }

    protected Instance(int layer, T symbol)
    {
        Id = Guid.NewGuid();
        Layer = layer;

        Symbol = symbol;

        Opacity = 1f;
    }

    protected Instance(Guid id, int layer, T symbol)
    {
        Id = id;
        Layer = layer;

        Symbol = symbol;

        Opacity = 1f;
    }

    public virtual int ZIndex(int index) => -1;
}

public interface Instance : IStorable
{
    public int Layer { get; set; }
    public float Opacity { get; set; }
    public Symbol Symbol { get; set; }
}