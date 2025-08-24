using System.Collections.Concurrent;

namespace Akade.IndexedSet.Utils;

/// <summary>
/// Concurrent object pool implementation with a fast single item slot for low contention scenarios
/// Based on Microsoft.Extensions.ObjectPool
/// </summary>
internal class ObjectPool<T> where T : class, new()
{
    public static ObjectPool<T> Instance { get; } = new();


    private readonly ConcurrentQueue<T> _values = new();
    private T? _fastItem;
    private int _count;
    private readonly int _maxCapacity = Environment.ProcessorCount * 2;

    public T Rent()
    {
        T? item = _fastItem;

        if (item is null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
        {
            if (_values.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _count);
                return item;
            }
            return new T();
        }

        return item;
    }

    public void Return(T item)
    {
        if (_fastItem is not null || Interlocked.CompareExchange(ref _fastItem, item, null) != null)
        {
            if (Interlocked.Increment(ref _count) <= _maxCapacity)
            {
                _values.Enqueue(item);
            }
            else
            {
                Interlocked.Decrement(ref _count);
            }
        }
    }
}
