using Akade.IndexedSet.Indices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet;

/// <summary>
/// Indexable data structure that allows to add different properties. Use <see cref="IndexedSetBuilder" /> and <see cref="IndexedSetBuilder{TElement}" /> to
/// create an instance. The set is not thread-safe.
/// </summary>
public class IndexedSet<TElement>
{
    private readonly HashSet<TElement> _data = new();
    private readonly Dictionary<string, Index<TElement>> _indices = new();

    /// <summary>
    /// Creates a new, empty instance of an <see cref="IndexedSet{TElement}"/>. 
    /// </summary>
    protected internal IndexedSet()
    {
    }



    /// <summary>
    /// Returns the number of elements currently within the set.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Adds a new item to the set. Use <see cref="AddRange(IEnumerable{TElement})"/> if you
    /// want to add multiple items at once.
    /// </summary>
    /// <param name="element">The new element to add</param>
    public bool Add(TElement element)
    {
        if (!_data.Add(element))
        {
            return false;
        }

        foreach (Index<TElement> index in _indices.Values)
        {
            index.Add(element);
        }

        return true;
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
        List<TElement> elementsToAdd;

        if (elements.TryGetNonEnumeratedCount(out int count))
        {
            elementsToAdd = new(count);
            _ = _data.EnsureCapacity(_data.Count + count);
        }
        else
        {
            elementsToAdd = new();
        }

        foreach (TElement element in elements)
        {
            if (_data.Add(element))
            {
                elementsToAdd.Add(element);
            }
        }

        foreach (Index<TElement> index in _indices.Values)
        {
            index.AddRange(elementsToAdd);
        }

        return elementsToAdd.Count;
    }

    /// <summary>
    /// Tries to remove an element from the set.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if an element was removed otherwise, false.</returns>
    public bool Remove(TElement element)
    {
        if (!_data.Remove(element))
        {
            return false;
        }

        foreach (Index<TElement> index in _indices.Values)
        {
            index.Remove(element);
        }
        return true;
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
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public bool TryGetSingle<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [NotNullWhen(true)] out TElement? element,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.TryGetSingle(indexKey, out element);
    }

    /// <summary>
    /// Searches for an element via an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
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
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Single(indexKey);
    }

    /// <summary>
    /// Searches for an element via an index  within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="contains">The key within the index. The convention is to write the parameter name for increased readability i.e. .Single(x => x.Prop, contains: value).</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public TElement Single<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Single(contains);
    }

    /// <summary>
    /// Searches for multiple elements via an index. See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, TIndexKey}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
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
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Where(indexKey);
    }

    /// <summary>
    /// Searches for multiple elements via an index within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="contains">The key within the index. The convention is to write the parameter name for increased readability i.e. .Where(x => x.Prop, contains: value).</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Where(contains);
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
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Range(start, end, inclusiveStart, inclusiveEnd);
    }

    /// <summary>
    /// Searches for elements that have keys that are less than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> LessThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are less or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> LessThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThanOrEqual(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greator than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> GreaterThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greator or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> GreaterThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThanOrEqual(value);
    }

    /// <summary>
    /// Searches for the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public TIndexKey Max<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Max();
    }

    /// <summary>
    /// Searches for the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public TIndexKey Min<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Min();
    }

    /// <summary>
    /// Searches for elements that have the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> MaxBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.MaxBy();
    }

    /// <summary>
    /// Searches for elements that have the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> MinBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.MinBy();
    }

    /// <summary>
    /// Returns the elements in the order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> OrderBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, int skip = 0, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.OrderBy(skip);
    }

    /// <summary>
    /// Returns the elements in the descending order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> OrderByDescending<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, int skip = 0, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.OrderByDescending(skip);
    }

    /// <summary>
    /// Returns all elements that start with the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> StartsWith(Func<TElement, ReadOnlyMemory<char>> indexAccessor, ReadOnlyMemory<char> prefix,  [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        TypedIndex<TElement, ReadOnlyMemory<char>> typedIndex = GetIndex<ReadOnlyMemory<char>>(indexName);
        return typedIndex.StartsWith(prefix);
    }

    /// <summary>
    /// Returns all elements that start with the given char sequence or a similar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input prefix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> FuzzyStartsWith(Func<TElement, ReadOnlyMemory<char>> indexAccessor, ReadOnlyMemory<char> prefix, int maxDistance, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        TypedIndex<TElement, ReadOnlyMemory<char>> typedIndex = GetIndex<ReadOnlyMemory<char>>(indexName);
        return typedIndex.FuzzyStartsWith(prefix, maxDistance);
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> Contains(Func<TElement, ReadOnlyMemory<char>> indexAccessor, ReadOnlyMemory<char> infix, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        TypedIndex<TElement, ReadOnlyMemory<char>> typedIndex = GetIndex<ReadOnlyMemory<char>>(indexName);
        return typedIndex.Contains(infix);
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence or a simalar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input infix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
    public IEnumerable<TElement> FuzzyContains(Func<TElement, ReadOnlyMemory<char>> indexAccessor, ReadOnlyMemory<char> infix, int maxDistance, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        TypedIndex<TElement, ReadOnlyMemory<char>> typedIndex = GetIndex<ReadOnlyMemory<char>>(indexName);
        return typedIndex.FuzzyContains(infix, maxDistance);
    }

    /// <summary>
    /// Returns all values by fully enumerating the entire set.
    /// </summary>
    public IEnumerable<TElement> FullScan()
    {
        return _data;
    }

    private TypedIndex<TElement, TIndexKey> GetIndex<TIndexKey>(string? indexName)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (!_indices.TryGetValue(indexName, out Index<TElement>? index))
        {
            throw new IndexNotFoundException(indexName);
        }

        var typedIndex = (TypedIndex<TElement, TIndexKey>)index;
        return typedIndex;
    }

    /// <summary>
    /// Returns true if the element is present in the current set, otherwise, false.
    /// </summary>
    public bool Contains(TElement element)
    {
        return _data.Contains(element);
    }

    internal void AddIndex(Index<TElement> index)
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


/// <summary>
/// Additionaly provides convienience access to a "primary key" unique index.
/// Functionally the same as manually adding a unique index on the primary key property.
/// </summary>
public class IndexedSet<TPrimaryKey, TElement> : IndexedSet<TElement>
    where TPrimaryKey : notnull
{
    private readonly Func<TElement, TPrimaryKey> _primaryKeyAccessor;
    private readonly string _primaryKeyIndexName;

    /// <summary>
    /// Creates a new, empty instance of an <see cref="IndexedSet{TPrimaryKey, TElement}"/>. 
    /// </summary>
    /// <param name="primaryKeyAccessor">Returns the primary key for a given item. The primary key should not be changed while the element is within the set. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="primaryKeyIndexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="primaryKeyIndexName">The name for the primary index. Usually, you should not specify this as the expression in <paramref name="primaryKeyIndexName"/> is automatically passed by the compiler.</param>
    protected internal IndexedSet(Func<TElement, TPrimaryKey> primaryKeyAccessor, [CallerArgumentExpression("primaryKeyAccessor")] string primaryKeyIndexName = "")
    {
        _primaryKeyAccessor = primaryKeyAccessor;
        _primaryKeyIndexName = primaryKeyIndexName;

        AddIndex(new UniqueIndex<TElement, TPrimaryKey>(_primaryKeyAccessor, primaryKeyIndexName));
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// Short-hand for <see cref="Single(TPrimaryKey)"/>.
    /// </summary>
    public TElement this[TPrimaryKey key] => Single(key);

    /// <summary>
    /// Attempts to remove an item with the given primary key and returns true, if one was found and removed. Otherwise, false.
    /// </summary>
    public bool Remove(TPrimaryKey key)
    {
        return TryGetSingle(_primaryKeyAccessor, key, out TElement? result, _primaryKeyIndexName) && Remove(result);
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// </summary>
    /// <param name="key">The primary key to obtain the item for</param>
    public TElement Single(TPrimaryKey key)
    {
        return Single(_primaryKeyAccessor, key, _primaryKeyIndexName);
    }

    /// <summary>
    /// Returns true if an element with the given primary key is present in the current set, otherwise, false.
    /// </summary>
    public bool Contains(TPrimaryKey key)
    {
        return Where(_primaryKeyAccessor, key, _primaryKeyIndexName).Any();
    }
}
