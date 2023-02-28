using OTools.Maps;
using System.Collections;
using System.ComponentModel;

namespace OTools.Course;

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

public sealed class ControlStore : BaseStore<Control>
{
    public ControlStore() { }

    public ControlStore(IEnumerable<Control> items)
        : base(items) { }
}

public sealed class CourseStore : BaseStore<Course>
{
    public CourseStore() { }

    public CourseStore(IEnumerable<Course> items)
        : base(items) { }
}

public sealed class ItemStore : BaseStore<Item>
{
    public ItemStore() { }

    public ItemStore(IEnumerable<Item> items)
        : base(items) { }
}