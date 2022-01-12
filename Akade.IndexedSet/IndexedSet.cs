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
    /// Searches for multiple elements via an index.
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
    /// Searches for multiple elements that are within a range via an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="inclusiveStart">The inclusive start "key" </param>
    /// <param name="exclusiveEnd">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> Range<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey inclusiveStart,
        TIndexKey exclusiveEnd,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TPrimaryKey, TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Range(inclusiveStart, exclusiveEnd);
    }

    /// <summary>
    /// Returns
    /// </summary>
    /// <returns></returns>
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
