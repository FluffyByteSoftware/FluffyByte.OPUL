namespace FluffyByte.OPUL.Tools.FluffyTypes;
public class ThreadSafeList<T>
{
    private readonly List<T> _items = [];
    private readonly Lock _lock = new();

    public void Add(T item)
    {
        lock (_lock)
        {
            _items.Add(item);
        }
    }

    public bool Remove(T item)
    {
        lock (_lock)
        {
            return _items.Remove(item);
        }
    }

    public T this[int index]
    {
        get { lock (_lock) return _items[index]; }
        set { lock (_lock) _items[index] = value; }
    }

    public int Count
    {
        get { lock (_lock) return _items.Count; }
    }

    public List<T> Snapshot()
    {
        lock (_lock)
        {
            // Return a shallow copy so external enumeration is safe
            return [.. _items];
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }
}
