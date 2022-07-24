using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;

/// <summary>
/// ReaderWriter-Lock based concurrency wrapper around <see cref="IndexedSet{TElement}" />.
/// Adds thread-safety, the trade off is, that all reading operations materialize the results into a collection.
/// 
/// </summary>
public class ConcurrentIndexedSet<TElement>
{
    private readonly AsyncReaderWriterLock _lock = new();
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
    /// Adds a new item to the set. Use <see cref="AddRangeAsync(IEnumerable{TElement}, CancellationToken)"/> if you
    /// want to add multiple items at once.
    /// </summary>
    /// <param name="element">The new element to add</param>
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<bool> AddAsync(TElement element, CancellationToken cancellationToken = default)
    {
        using (await AcquireWriterLockAsync(cancellationToken))
        {
            return _indexedSet.Add(element);
        }
    }

    /// <summary>
    /// Adds multiple elements at once. In contrast to <see cref="AddAsync(TElement, CancellationToken)"/>, this method
    /// allows indices to perform the insertion in a preferable way, for example, by ordering
    /// the elements prior to insertion.
    /// </summary>
    /// <param name="elements">The elements to insert</param>
    /// <returns>Returns the number of inserted elements</returns>
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<int> AddRangeAsync(IEnumerable<TElement> elements, CancellationToken cancellationToken = default)
    {
        using (await AcquireWriterLockAsync(cancellationToken))
        {
            return _indexedSet.AddRange(elements);
        }
    }

    /// <summary>
    /// Tries to remove an element from the set.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if an element was removed otherwise, false.</returns>
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<bool> RemoveAsync(TElement element, CancellationToken cancellationToken = default)
    {
        using (await AcquireWriterLockAsync(cancellationToken))
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
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<TElement?> TryGetSingleAsync<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
        {
            _ = _indexedSet.TryGetSingle(indexAccessor, indexKey, out TElement? result, indexName);
            return result;
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<TElement> SingleAsync<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<TElement> SingleAsync<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> WhereAsync<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> WhereAsync<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> RangeAsync<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart = true,
        bool inclusiveEnd = false,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> LessThanAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> LessThanOrEqualAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> GreaterThanAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> GreaterThanOrEqualAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<TIndexKey> MaxAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<TIndexKey> MinAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> MaxByAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> MinByAsync<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> OrderByAsync<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        int skip = 0,
        int count = -1,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> OrderByDescendingAsync<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        int skip = 0,
        int count = -1,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
        where TIndexKey : notnull
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> StartsWithAsync(
        Func<TElement, ReadOnlyMemory<char>> indexAccessor,
        ReadOnlyMemory<char> prefix,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> FuzzyStartsWithAsync(
        Func<TElement, ReadOnlyMemory<char>> indexAccessor,
        ReadOnlyMemory<char> prefix,
        int maxDistance,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> ContainsAsync(
        Func<TElement, ReadOnlyMemory<char>> indexAccessor,
        ReadOnlyMemory<char> infix,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        using (await AcquireReaderLockAsync(cancellationToken))
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
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<IEnumerable<TElement>> FuzzyContainsAsync(
        Func<TElement, ReadOnlyMemory<char>> indexAccessor,
        ReadOnlyMemory<char> infix,
        int maxDistance,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        using (await AcquireReaderLockAsync(cancellationToken))
        {
            return _indexedSet.FuzzyContains(indexAccessor, infix, maxDistance, indexName).ToList();
        }
    }

    /// <summary>
    /// Internal method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ValueTask<IDisposable> AcquireWriterLockAsync(CancellationToken cancellationToken)
    {
        return _lock.AcquireWriterLockAsync(cancellationToken);
    }

    /// <summary>
    /// Internal method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ValueTask<IDisposable> AcquireReaderLockAsync(CancellationToken cancellationToken)
    {
        return _lock.AcquireReaderLockAsync(cancellationToken);
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
    public async ValueTask<bool> RemoveAsync(TPrimaryKey key, CancellationToken cancellationToken = default)
    {
        using (await AcquireWriterLockAsync(cancellationToken))
        {
            return _primaryKeyIndexedSet.Remove(key);
        }
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// </summary>
    /// <param name="key">The primary key to obtain the item for</param>
    /// <param name="cancellationToken">Cancellation token to cancel any potential concurrent wait.</param>
    public async ValueTask<TElement> SingleAsync(TPrimaryKey key, CancellationToken cancellationToken = default)
    {
        using (await AcquireReaderLockAsync(cancellationToken))
        {
            return _primaryKeyIndexedSet.Single(key);
        }
    }

    /// <summary>
    /// Returns true if an element with the given primary key is present in the current set, otherwise, false.
    /// </summary>
    public async ValueTask<bool> ContainsAsync(TPrimaryKey key, CancellationToken cancellationToken = default)
    {
        using (await AcquireReaderLockAsync(cancellationToken))
        {
            return _primaryKeyIndexedSet.Contains(key);
        }
    }
}
