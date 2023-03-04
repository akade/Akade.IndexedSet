using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;

/// <summary>
/// ReaderWriter-Lock based concurrency wrapper around <see cref="IndexedSet{TElement}" />.
/// Adds thread-safety, the trade off is, that all reading operations materialize the results into a collection.
/// 
/// </summary>
#if NET7_0_OR_GREATER
[SuppressMessage("Style", "IDE0280:Use 'nameof'", Justification = ".NET 6 is still supported")]
#endif
public class ConcurrentIndexedSet<TElement> : IDisposable
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
    public int Count => _indexedSet.Count;

    /// <summary>
    /// Adds a new item to the set. Use <see cref="AddRange(IEnumerable{TElement})"/> if you
    /// want to add multiple items at once.
    /// </summary>
    /// <param name="element">The new element to add</param>
    public bool Add(TElement element)
    {
        using (AcquireWriterLock())
        {
            return _indexedSet.Add(element);
        }
    }

    /// <summary>
    /// Adds multiple elements at once. In contrast to <see cref="Add(TElement)"/>, this method
    /// allows indices to perform the insertion in a preferable way, for example, by ordering
    /// the elements prior to insertion.
    /// </summary>
    /// <param name="elements">The elements to insert</param>
    /// <returns>Returns the number of inserted elements</returns>
    public int AddRange(IEnumerable<TElement> elements)
    {
        using (AcquireWriterLock())
        {
            return _indexedSet.AddRange(elements);
        }
    }

    /// <summary>
    /// Tries to remove an element from the set.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if an element was removed otherwise, false.</returns>
    public bool Remove(TElement element)
    {
        using (AcquireWriterLock())
        {
            return _indexedSet.Remove(element);
        }
    }

    /// <summary>
    /// Searches for an element via an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="element">The element if found, otherwise null.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public bool TryGetSingle<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [NotNullWhen(true)] out TElement? element,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.TryGetSingle(indexAccessor, indexKey, out element, indexName);
        }
    }

    /// <summary>
    /// Searches for an element via an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public TElement Single<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Single(indexAccessor, indexKey, indexName);
        }
    }

    /// <summary>
    /// Searches for an element via an index  within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="contains">The key within the index. The convention is to write the parameter name for increased readability i.e. .Single(x => x.Prop, contains: value).</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public TElement Single<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Single(indexAccessor, contains, indexName);
        }
    }

    /// <summary>
    /// Searches for multiple elements via an index. See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, TIndexKey}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Where(indexAccessor, indexKey, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for multiple elements via an index within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="contains">The key within the index. The convention is to write the parameter name for increased readability i.e. .Where(x => x.Prop, contains: value).</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Where(indexAccessor, contains, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for multiple elements that are within a range via an index. You can specify whether the start/end are inclusive or exclusive
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="start">The start "key", use <paramref name="inclusiveStart"/> to control whether the result should include the start or not </param>
    /// <param name="end">The end "key", use <paramref name="inclusiveEnd"/> to control whether the result should include the end or not </param>
    /// <param name="inclusiveStart">True, if the query should include the start, otherwise false.</param>
    /// <param name="inclusiveEnd">True, if the query should include the end, otherwise false.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> Range<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart = true,
        bool inclusiveEnd = false,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Range(indexAccessor, start, end, inclusiveStart, inclusiveEnd, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for elements that have keys that are less than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> LessThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.LessThan(indexAccessor, value, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for elements that have keys that are less or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> LessThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.LessThanOrEqual(indexAccessor, value, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for elements that have keys that are greator than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> GreaterThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.GreaterThan(indexAccessor, value, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for elements that have keys that are greator or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> GreaterThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.GreaterThanOrEqual(indexAccessor, value, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public TIndexKey Max<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Max(indexAccessor, indexName);
        }
    }

    /// <summary>
    /// Searches for the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public TIndexKey Min<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Min(indexAccessor, indexName);
        }
    }

    /// <summary>
    /// Searches for elements that have the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> MaxBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.MaxBy(indexAccessor, indexName).ToList();
        }
    }

    /// <summary>
    /// Searches for elements that have the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> MinBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.MinBy(indexAccessor, indexName).ToList();
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
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
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
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
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
    /// Returns all elements that start with the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> StartsWith(
        Func<TElement, string> indexAccessor,
        ReadOnlySpan<char> prefix,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.StartsWith(indexAccessor, prefix, indexName).ToList();
        }
    }

    /// <summary>
    /// Returns all elements that start with the given char sequence or a similar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input prefix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> FuzzyStartsWith(
        Func<TElement, string> indexAccessor,
        ReadOnlySpan<char> prefix,
        int maxDistance,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.FuzzyStartsWith(indexAccessor, prefix, maxDistance, indexName).ToList();
        }
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> Contains(
        Func<TElement, string> indexAccessor,
        ReadOnlySpan<char> infix,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Contains(indexAccessor, infix, indexName).ToList();
        }
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence or a simalar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input infix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public IEnumerable<TElement> FuzzyContains(
        Func<TElement, string> indexAccessor,
        ReadOnlySpan<char> infix,
        int maxDistance,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.FuzzyContains(indexAccessor, infix, maxDistance, indexName).ToList();
        }
    }

    /// <summary>
    /// Removes all elements
    /// </summary>
    public void Clear()
    {
        using (AcquireWriterLock())
        {
            _indexedSet.Clear();
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
    public void Update<TState>(Action<IndexedSet<TElement>, TState> updateFunc, TState state)
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
public class ConcurrentIndexedSet<TPrimaryKey, TElement> : ConcurrentIndexedSet<TElement>
    where TPrimaryKey : notnull
{
    private readonly IndexedSet<TPrimaryKey, TElement> _primaryKeyIndexedSet;

    internal ConcurrentIndexedSet(IndexedSet<TPrimaryKey, TElement> indexedSet) : base(indexedSet)
    {
        _primaryKeyIndexedSet = indexedSet;
    }

    /// <summary>
    /// Attempts to remove an item with the given primary key and returns true, if one was found and removed. Otherwise, false.
    /// </summary>
    public bool Remove(TPrimaryKey key)
    {
        using (AcquireWriterLock())
        {
            return _primaryKeyIndexedSet.Remove(key);
        }
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// </summary>
    /// <param name="key">The primary key to obtain the item for</param>
    public TElement Single(TPrimaryKey key)
    {
        using (AcquireReaderLock())
        {
            return _primaryKeyIndexedSet.Single(key);
        }
    }

    /// <summary>
    /// Returns true if an element with the given primary key is present in the current set, otherwise, false.
    /// </summary>
    public bool Contains(TPrimaryKey key)
    {
        using (AcquireReaderLock())
        {
            return _primaryKeyIndexedSet.Contains(key);
        }
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
