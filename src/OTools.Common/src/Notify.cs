namespace OTools.Common;

public sealed class Notify<T>
{
    private T _value;

    public Notify(T value)
    {
        _value = value;
    }

    public event Action<NotifyEventArgs<T>>? OnChanged;
    

    public T GetValue()
    {
        return _value;
    }

    public void SetValue(T value)
    {
        OnChanged?.Invoke(new(_value, value));
        _value = value;
    }

    public static implicit operator T(Notify<T> notify)
    {
        return notify._value;
    }

    //public static implicit operator Notify<T>(T value)
}

public struct NotifyEventArgs<T>
{
    public T OldValue { get; }
    public T NewValue { get; }

    internal NotifyEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}