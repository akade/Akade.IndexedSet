using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;

/// <summary>
/// ReaderWriter-Lock based concurrency wrapper around <see cref="IndexedSet{TElement}" />.
/// Adds thread-safety, the trade off is, that all reading operations materialize the results into a collection.
/// 
/// </summary>
public partial class ConcurrentIndexedSet<TElement> : IDisposable
{
    private readonly ReaderWriterLockEx _lock = new();
    private readonly IndexedSet<TElement> _indexedSet;

    internal ConcurrentIndexedSet(IndexedSet<TElement> indexedSet)
    {
        _indexedSet = indexedSet;
    }

    /// <summary>
    /// Returns the number of elements currently within the set.
    /// </summary>
    public int Count
    {
        get
        {
            using (AcquireReaderLock())
            {
                return _indexedSet.Count;
            }
        }
    }

    /// <summary>
    /// Returns the elements in the order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="count">The concurrent implementation performs enumeration on the results. If this value is bigger than zero, at max the specified amount of items is returned.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> OrderBy<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        int skip = 0,
        int count = -1,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            IEnumerable<TElement> result = _indexedSet.OrderBy(indexAccessor, skip, indexName);

            if (count > 0)
            {
                result = result.Take(count);
            }

            return result.ToList();
        }
    }


    /// <summary>
    /// Returns the elements in the order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="count">The concurrent implementation performs enumeration on the results. If this value is bigger than zero, at max the specified amount of items is returned.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> OrderBy<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        int skip = 0,
        int count = -1,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            IEnumerable<TElement> result = _indexedSet.OrderBy(indexAccessor, skip, indexName);

            if (count > 0)
            {
                result = result.Take(count);
            }

            return result.ToList();
        }
    }


    /// <summary>
    /// Returns the elements in the descending order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="count">The concurrent implementation performs enumeration on the results. If this value is bigger than zero, at max the specified amount of items is returned.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> OrderByDescending<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        int skip = 0,
        int count = -1,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            IEnumerable<TElement> result = _indexedSet.OrderByDescending(indexAccessor, skip, indexName);

            if (count > 0)
            {
                result = result.Take(count);
            }

            return result.ToList();
        }
    }

    /// <summary>
    /// Returns the elements in the descending order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="count">The concurrent implementation performs enumeration on the results. If this value is bigger than zero, at max the specified amount of items is returned.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> OrderByDescending<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        int skip = 0,
        int count = -1,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            IEnumerable<TElement> result = _indexedSet.OrderByDescending(indexAccessor, skip, indexName);

            if (count > 0)
            {
                result = result.Take(count);
            }

            return result.ToList();
        }
    }

    /// <summary>
    /// Allows to update the underlying indexed set while being protected by the write lock
    /// </summary>
    /// <param name="updateFunc">Update function</param>
    public void Update(Action<IndexedSet<TElement>> updateFunc)
    {
        using (AcquireWriterLock())
        {
            updateFunc(_indexedSet);
        }
    }

    /// <summary>
    /// Allows to update the underlying indexed set while being protected by the write lock
    /// </summary>
    /// <param name="updateFunc">Update function</param>
    /// <param name="state">User defined state that is passed to <paramref name="updateFunc"/>.</param>
    [Obsolete("May be removed in a future version.")]
    public void Update<TState>(Action<IndexedSet<TElement>, TState> updateFunc, TState state)
    {
        using (AcquireWriterLock())
        {
            updateFunc(_indexedSet, state);
        }
    }

    /// <summary>
    /// Allows to update the underlying indexed set while being protected by the write lock
    /// </summary>
    /// <param name="updateFunc">Update function</param>
    /// <param name="state">User defined state that is passed to <paramref name="updateFunc"/>.</param>
    public void Update<TState>(TState state, Action<IndexedSet<TElement>, TState> updateFunc)
    {
        using (AcquireWriterLock())
        {
            updateFunc(_indexedSet, state);
        }
    }

    /// <summary>
    /// Internal method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IDisposable AcquireWriterLock()
    {
        return _lock.EnterWriteLock();
    }

    /// <summary>
    /// Internal method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IDisposable AcquireReaderLock()
    {
        return _lock.EnterReadLock();
    }

    /// <summary>
    /// Allows access to the underlying indexed set while being protected by a read lock. Use it if you need
    /// to query based on different indices, only paying the materialized collection cost for the returned results.
    /// Accessing any update methods may result in undefined behavior. 
    /// </summary>
    /// <param name="readFunc">Read function: Do not materialize the results</param>
    /// <returns>The materialized results returned by <paramref name="readFunc"/></returns>
    public IEnumerable<TElement> Read(Func<IndexedSet<TElement>, IEnumerable<TElement>> readFunc)
    {
        using (AcquireReaderLock())
        {
            return readFunc(_indexedSet).ToArray();
        }
    }

    /// <summary>
    /// Allows access to the underlying indexed set while being protected by a read lock. Use it if you need
    /// to query based on different indices, only paying the materialized collection cost for the returned results.
    /// Accessing any update methods may result in undefined behavior. 
    /// </summary>
    /// <param name="readFunc">Read function: Do not materialize the results</param>
    /// <param name="state">State passed to the read function</param>
    /// <returns>The materialized results returned by <paramref name="readFunc"/></returns>
    public IEnumerable<TElement> Read<TState>(TState state, Func<IndexedSet<TElement>, TState, IEnumerable<TElement>> readFunc)
    {
        using (AcquireReaderLock())
        {
            return readFunc(_indexedSet, state).ToArray();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
/// <summary>
/// ReaderWriter-Lock based concurrency wrapper around <see cref="IndexedSet{TPrimaryKey, TElement}" />
/// </summary>
public partial class ConcurrentIndexedSet<TPrimaryKey, TElement> : ConcurrentIndexedSet<TElement>
    where TPrimaryKey : notnull
{
    private readonly IndexedSet<TPrimaryKey, TElement> _primaryKeyIndexedSet;

    internal ConcurrentIndexedSet(IndexedSet<TPrimaryKey, TElement> indexedSet) : base(indexedSet)
    {
        _primaryKeyIndexedSet = indexedSet;
    }

    /// <summary>
    /// Allows to update the underlying indexed set while being protected by the write lock
    /// </summary>
    /// <param name="updateFunc">Update function</param>
    public void Update(Action<IndexedSet<TPrimaryKey, TElement>> updateFunc)
    {
        using (AcquireWriterLock())
        {
            updateFunc(_primaryKeyIndexedSet);
        }
    }

    /// <summary>
    /// Allows to update the underlying indexed set while being protected by the write lock
    /// </summary>
    /// <param name="updateFunc">Update function</param>
    /// <param name="state">User defined state that is passed to <paramref name="updateFunc"/>.</param>
    public void Update<TState>(Action<IndexedSet<TPrimaryKey, TElement>, TState> updateFunc, TState state)
    {
        using (AcquireWriterLock())
        {
            updateFunc(_primaryKeyIndexedSet, state);
        }
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// Short-hand for <see cref="Single(TPrimaryKey)"/>.
    /// </summary>
    public TElement this[TPrimaryKey key] => Single(key);
}
