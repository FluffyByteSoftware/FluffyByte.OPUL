using System.Collections;

namespace FluffyByte.OPUL.Tools.FluffyTypes;

/// <summary>
/// A thread-safe wrapper around List<T> that provides safe concurrent access.
/// Enumeration returns a snapshot to prevent concurrent modification exceptions.
/// </summary>
public class ThreadSafeList<T> : IEnumerable<T>
{
    private readonly List<T> _items = [];
    private readonly Lock _lock = new();

    #region Adding Items

    public void Add(T item)
    {
        lock (_lock)
        {
            _items.Add(item);
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        lock (_lock)
        {
            _items.AddRange(collection);
        }
    }

    public void Insert(int index, T item)
    {
        lock (_lock)
        {
            _items.Insert(index, item);
        }
    }

    #endregion

    #region Removing Items

    public bool Remove(T item)
    {
        lock (_lock)
        {
            return _items.Remove(item);
        }
    }

    public void RemoveAt(int index)
    {
        lock (_lock)
        {
            _items.RemoveAt(index);
        }
    }

    public int RemoveAll(Predicate<T> match)
    {
        lock (_lock)
        {
            return _items.RemoveAll(match);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }

    #endregion

    #region Searching

    public bool Contains(T item)
    {
        lock (_lock)
        {
            return _items.Contains(item);
        }
    }

    public int IndexOf(T item)
    {
        lock (_lock)
        {
            return _items.IndexOf(item);
        }
    }

    public T? Find(Predicate<T> match)
    {
        lock (_lock)
        {
            return _items.Find(match);
        }
    }

    public List<T> FindAll(Predicate<T> match)
    {
        lock (_lock)
        {
            return _items.FindAll(match);
        }
    }

    public bool Exists(Predicate<T> match)
    {
        lock (_lock)
        {
            return _items.Exists(match);
        }
    }

    #endregion

    #region Sorting and Ordering

    public void Sort()
    {
        lock (_lock)
        {
            _items.Sort();
        }
    }

    public void Sort(Comparison<T> comparison)
    {
        lock (_lock)
        {
            _items.Sort(comparison);
        }
    }

    public void Reverse()
    {
        lock (_lock)
        {
            _items.Reverse();
        }
    }

    #endregion

    #region Access

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// Note: This operation locks briefly. For bulk reads, consider using Snapshot() or ForEach().
    /// </summary>
    public T this[int index]
    {
        get
        {
            lock (_lock)
                return _items[index];
        }
        set
        {
            lock (_lock)
                _items[index] = value;
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
                return _items.Count;
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Returns a shallow copy of the list for safe external enumeration.
    /// Use this when you need to iterate without holding the lock.
    /// </summary>
    public List<T> Snapshot()
    {
        lock (_lock)
        {
            return [.. _items];
        }
    }

    /// <summary>
    /// Executes an action on each element while holding the lock.
    /// More efficient than Snapshot() for simple iterations.
    /// WARNING: Don't perform lengthy operations inside the action!
    /// </summary>
    public void ForEach(Action<T> action)
    {
        lock (_lock)
        {
            foreach (var item in _items)
            {
                action(item);
            }
        }
    }

    /// <summary>
    /// Copies all elements to an array.
    /// </summary>
    public T[] ToArray()
    {
        lock (_lock)
        {
            return [];
        }
    }

    #endregion

    #region IEnumerable Implementation

    /// <summary>
    /// Returns an enumerator over a snapshot of the list.
    /// This prevents concurrent modification exceptions but means changes 
    /// during enumeration won't be reflected.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        // Return enumerator over a snapshot to ensure thread safety
        return Snapshot().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}