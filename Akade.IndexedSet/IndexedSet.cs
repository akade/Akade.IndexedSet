using Akade.IndexedSet.Indices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet;

/// <summary>
/// Indexable data structure that provides O(1) access to elements via the primary key and allows
/// to additionaly index other properties. Use <see cref="IndexedSetBuilder{TElement}.Create{TPrimaryKey}(Func{TElement, TPrimaryKey})" /> to
/// create an instance. The set is not thread-safe.
/// </summary>
public class IndexedSet<TPrimaryKey, TElement> where TPrimaryKey : notnull
{
    private readonly Dictionary<TPrimaryKey, TElement> _data = new();
    private readonly Dictionary<string, Index<TPrimaryKey, TElement>> _indices = new();
    private readonly Func<TElement, TPrimaryKey> _primaryKeyAccessor;

    /// <summary>
    /// Creates a new, empty instance of an <see cref="IndexedSet{TPrimaryKey, TElement}"/>. 
    /// </summary>
    /// <param name="primaryKeyAccessor">Returns the primary key for a given item. The primary key should not be changed while the element is within the set.</param>
    public IndexedSet(Func<TElement, TPrimaryKey> primaryKeyAccessor)
    {
        _primaryKeyAccessor = primaryKeyAccessor;
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// Short-hand for <see cref="Single(TPrimaryKey)"/>.
    /// </summary>
    public TElement this[TPrimaryKey key] => Single(key);

    /// <summary>
    /// Returns the number of elements currently within the set.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Adds a new item to the set.
    /// </summary>
    /// <param name="element">The new element to add</param>
    public void Add(TElement element)
    {
        TPrimaryKey key = _primaryKeyAccessor(element);
        _data.Add(key, element);

        foreach (Index<TPrimaryKey, TElement> index in _indices.Values)
        {
            index.Add(element);
        }
    }

    /// <summary>
    /// Tries to remove an element from the set.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if an element was removed otherwise, false.</returns>
    public bool Remove(TElement element)
    {
        TPrimaryKey key = _primaryKeyAccessor(element);
        return Remove(key);
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// </summary>
    /// <param name="key">The primary key to obtain the item for</param>
    public TElement Single(TPrimaryKey key)
    {
        return _data[key];
    }

    /// <summary>
    /// Searches for an element via an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public TElement Single<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Single(indexKey);
    }

    /// <summary>
    /// Searches for multiple elements via an index. See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, TIndexKey}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Where(indexKey);
    }

    /// <summary>
    /// Searches for multiple elements via an index within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Where(indexKey);
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
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> Range<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart = true,
        bool inclusiveEnd = false,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Range(start, end, inclusiveStart, inclusiveEnd);
    }

    /// <summary>
    /// Searches for elements that have keys that are less than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> LessThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are less or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> LessThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThanOrEqual(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greator than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> GreaterThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greator or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> GreaterThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThanOrEqual(value);
    }

    /// <summary>
    /// Searches for the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public TIndexKey Max<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Max();
    }

    /// <summary>
    /// Searches for the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public TIndexKey Min<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Min();
    }

    /// <summary>
    /// Searches for elements that have the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> MaxBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.MaxBy();
    }

    /// <summary>
    /// Searches for elements that have the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> MinBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.MinBy();
    }

    /// <summary>
    /// Returns the elements in the order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> OrderBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, int skip = 0, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.OrderBy(skip);
    }

    /// <summary>
    /// Returns the elements in the descending order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identiefer. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> OrderByDescending<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, int skip = 0, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.OrderByDescending(skip);
    }

    /// <summary>
    /// Returns all values by fully enumerating the entire set.
    /// </summary>
    public IEnumerable<TElement> FullScan()
    {
        return _data.Values;
    }



    private TypedIndex<TPrimaryKey, TElement, TIndexKey> GetIndex<TIndexKey>(string? indexName)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (!_indices.TryGetValue(indexName, out Index<TPrimaryKey, TElement>? index))
        {
            throw new IndexNotFoundException(indexName);
        }

        var typedIndex = (TypedIndex<TPrimaryKey, TElement, TIndexKey>)index;
        return typedIndex;
    }

    /// <summary>
    /// Attempts to remove an item with the given primary key and returns true, if one was found and removed. Otherwise, false.
    /// </summary>
    public bool Remove(TPrimaryKey key)
    {
        if (_data.TryGetValue(key, out TElement? element))
        {
            _ = _data.Remove(key);
            foreach (Index<TPrimaryKey, TElement> index in _indices.Values)
            {
                index.Remove(element);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns true if the element is present in the current set, otherwise, false. Note: This is equivalent to
    /// calling <see cref="Contains(TPrimaryKey)"/> with the primary key and does not perform
    /// an equality check on <paramref name="element"/>!
    /// </summary>
    public bool Contains(TElement element)
    {
        TPrimaryKey key = _primaryKeyAccessor(element);
        return Contains(key);
    }

    /// <summary>
    /// Returns true if an element with the given primary key is present in the current set, otherwise, false.
    /// </summary>
    public bool Contains(TPrimaryKey key)
    {
        return _data.ContainsKey(key);
    }

    internal void AddIndex(Index<TPrimaryKey, TElement> index)
    {
        ThrowIfNonEmpty();
        _indices.Add(index.Name, index);
    }

    private void ThrowIfNonEmpty()
    {
        if (_data.Count > 0)
        {
            throw new InvalidOperationException("The operation is not allowed if the set is not empty.");
        }
    }
}
