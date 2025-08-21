// Ignore Spelling: Accessor

using Akade.IndexedSet.Concurrency;
using Akade.IndexedSet.Indices;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet;

/// <summary>
/// Indexable data structure that allows to add different properties. Use <see cref="IndexedSetBuilder" /> and <see cref="IndexedSetBuilder{TElement}" /> to
/// create an instance. The set is not thread-safe.
/// </summary>
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used as caller argument expression")]
public class IndexedSet<TElement>
{
    private readonly HashSet<TElement> _data = [];
    private FrozenDictionary<string, Index<TElement>> _indices = FrozenDictionary<string, Index<TElement>>.Empty;
    private FrozenDictionary<string, IndexWriter<TElement>> _indexWriters = FrozenDictionary<string, IndexWriter<TElement>>.Empty;
    
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
    [WriteAccess]
    public bool Add(TElement element)
    {
        if (!_data.Add(element))
        {
            return false;
        }

        try
        {
            foreach (IndexWriter<TElement> writer in _indexWriters.Values)
            {
                writer.Add(element);
            }
        }
        catch
        {
            _ = Remove(element);

            throw;
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
    [WriteAccess]
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
            elementsToAdd = [];
        }

        foreach (TElement element in elements)
        {
            if (_data.Add(element))
            {
                elementsToAdd.Add(element);
            }
        }

        try
        {
            foreach (IndexWriter<TElement> writer in _indexWriters.Values)
            {
                writer.AddRange(elements);
            }
        }
        catch
        {
            foreach (TElement element in elementsToAdd)
            {
                _ = Remove(element);
            }
            throw;
        }

        return elementsToAdd.Count;
    }

    /// <summary>
    /// Tries to remove an element from the set.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if an element was removed otherwise, false.</returns>
    [WriteAccess]
    public bool Remove(TElement element)
    {
        if (!_data.Remove(element))
        {
            return false;
        }

        foreach (IndexWriter<TElement> writer in _indexWriters.Values)
        {
            writer.Remove(element);
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
    [ReadAccess]
    public bool TryGetSingle<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [NotNullWhen(true)] out TElement? element,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    /// <param name="element">The element if found, otherwise null.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    [ReadAccess]
    public bool TryGetSingle<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey indexKey,
        [NotNullWhen(true)] out TElement? element,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public TElement Single<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Single(indexKey);
    }

    /// <summary>
    /// Searches for an element via an index  within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, IEqualityComparer{TIndexKey}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="contains">The key within the index. The convention is to write the parameter name for increased readability i.e. .Single(x => x.Prop, contains: value).</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public TElement Single<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Single(contains);
    }

    /// <summary>
    /// Searches for multiple elements via an index. See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, TIndexKey}, IEqualityComparer{TIndexKey}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Where(indexKey);
    }

    /// <summary>
    /// Searches for multiple elements via an index within "denormalized keys". See <see cref="IndexedSetBuilder{TPrimaryKey, TElement}.WithIndex{TIndexKey}(Func{TElement, IEnumerable{TIndexKey}}, IEqualityComparer{TIndexKey}, string?)"/>.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="contains">The key within the index. The convention is to write the parameter name for increased readability i.e. .Where(x => x.Prop, contains: value).</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> Where<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey contains,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public IEnumerable<TElement> Range<TIndexKey>(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart = true,
        bool inclusiveEnd = false,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Range(start, end, inclusiveStart, inclusiveEnd);
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
    [ReadAccess]
    public IEnumerable<TElement> Range<TIndexKey>(
        Func<TElement, IEnumerable<TIndexKey>> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart = true,
        bool inclusiveEnd = false,
        [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public IEnumerable<TElement> LessThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are less than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> LessThan<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public IEnumerable<TElement> LessThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThanOrEqual(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are less or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> LessThanOrEqual<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.LessThanOrEqual(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greater than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> GreaterThan<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greater than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> GreaterThan<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThan(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greater or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> GreaterThanOrEqual<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.GreaterThanOrEqual(value);
    }

    /// <summary>
    /// Searches for elements that have keys that are greater or equal than the supplied value.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="value">The key value to compare other keys with.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> GreaterThanOrEqual<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public TIndexKey Max<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Max();
    }

    /// <summary>
    /// Searches for the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public TIndexKey Max<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public TIndexKey Min<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.Min();
    }

    /// <summary>
    /// Searches for the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public TIndexKey Min<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public IEnumerable<TElement> MaxBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.MaxBy();
    }

    /// <summary>
    /// Searches for elements that have the maximum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> MaxBy<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public IEnumerable<TElement> MinBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.MinBy();
    }

    /// <summary>
    /// Searches for elements that have the minimum key value within an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> MinBy<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    public IEnumerable<TElement> OrderBy<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, int skip = 0, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.OrderBy(skip);
    }

    /// <summary>
    /// Returns the elements in the order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    public IEnumerable<TElement> OrderBy<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, int skip = 0, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    public IEnumerable<TElement> OrderByDescending<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, int skip = 0, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.OrderByDescending(skip);
    }

    /// <summary>
    /// Returns the elements in the descending order defined by the index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="skip">Allows to efficiently skip a number of elements. Default is 0</param>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    public IEnumerable<TElement> OrderByDescending<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, int skip = 0, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
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
    [ReadAccess]
    public IEnumerable<TElement> StartsWith(Func<TElement, string> indexAccessor, ReadOnlySpan<char> prefix, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.StartsWith(prefix);
    }

    /// <summary>
    /// Returns all elements that start with the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public IEnumerable<TElement> StartsWith(Func<TElement, IEnumerable<string>> indexAccessor, ReadOnlySpan<char> prefix, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
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
    [ReadAccess]
    public IEnumerable<TElement> FuzzyStartsWith(Func<TElement, string> indexAccessor, ReadOnlySpan<char> prefix, int maxDistance, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.FuzzyStartsWith(prefix, maxDistance);
    }

    /// <summary>
    /// Returns all elements that start with the given char sequence or a similar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input prefix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public IEnumerable<TElement> FuzzyStartsWith(Func<TElement, IEnumerable<string>> indexAccessor, ReadOnlySpan<char> prefix, int maxDistance, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.FuzzyStartsWith(prefix, maxDistance);
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> Contains(Func<TElement, string> indexAccessor, ReadOnlySpan<char> infix, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.Contains(infix);
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public IEnumerable<TElement> Contains(Func<TElement, IEnumerable<string>> indexAccessor, ReadOnlySpan<char> infix, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.Contains(infix);
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence or a similar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input infix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> FuzzyContains(Func<TElement, string> indexAccessor, ReadOnlySpan<char> infix, int maxDistance, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.FuzzyContains(infix, maxDistance);
    }

    /// <summary>
    /// Returns all elements that contain the given char sequence or a similar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input infix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public IEnumerable<TElement> FuzzyContains(Func<TElement, IEnumerable<string>> indexAccessor, ReadOnlySpan<char> infix, int maxDistance, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
        return typedIndex.FuzzyContains(infix, maxDistance);
    }


    /// <summary>
    /// Returns the nearest neighbors of the given value, lazily enumerating from the closest to the furthest neighbors.
    /// Currently supports <see cref="Vector2"/> and <see cref="Vector3"/> as index keys.
    /// </summary>
    /// <typeparam name="TIndexKey"><see cref="Vector2"/> or <see cref="Vector3"/> </typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. </param>
    /// <param name="value">The point to search the closest neighbors from</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [ReadAccess]
    public IEnumerable<TElement> NearestNeighbors<TIndexKey>(Func<TElement, TIndexKey> indexAccessor, TIndexKey value, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
        return typedIndex.NearestNeighbors(value);
    }

    /// <summary>
    /// Returns all values by fully enumerating the entire set.
    /// </summary>
    public IEnumerable<TElement> FullScan()
    {
        return _data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TypedIndex<TElement, TIndexKey> GetIndex<TIndexKey>(string? indexName)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);

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
    [ReadAccess]
    public bool Contains(TElement element)
    {
        return _data.Contains(element);
    }

    /// <summary>
    /// Removes all elements
    /// </summary>
    [WriteAccess]
    public void Clear()
    {
        _data.Clear();
        foreach (Index<TElement> index in _indices.Values)
        {
            index.Clear();
        }
    }

    /// <summary>
    /// Helper function that allows to safely update mutable, indexed keys within <paramref name="updateFunc"/>.
    /// </summary>
    /// <param name="element">The element to update. Is passed to <paramref name="updateFunc"/>.</param>
    /// <param name="updateFunc">The update function</param>
    /// <returns>True, if the element was present before. False if the element was added.</returns>
    [WriteAccess]
    public bool Update(TElement element, Action<TElement> updateFunc)
    {
        bool result = Remove(element);
        updateFunc(element);
        _ = Add(element);
        return result;
    }

    /// <summary>
    /// Helper function that allows to safely update mutable, indexed keys within <paramref name="updateFunc"/>.
    /// </summary>
    /// <param name="element">The element to update. Is passed to <paramref name="updateFunc"/>.</param>
    /// <param name="state">User defined state that is passed to <paramref name="updateFunc"/>.</param>
    /// <param name="updateFunc">The update function</param>
    /// <returns>True, if the element was present before. False if the element was added.</returns>
    [WriteAccess]
    public bool Update<TState>(TElement element, TState state, Action<TElement, TState> updateFunc)
    {
        bool result = Remove(element);
        updateFunc(element, state);
        _ = Add(element);
        return result;
    }

    /// <summary>
    /// Helper function that allows to safely update immutable, indexed keys within <paramref name="updateFunc"/>.
    /// </summary>
    /// <param name="element">The element to update. Is passed to <paramref name="updateFunc"/>.</param>
    /// <param name="updateFunc">The update function, can return the same or a new instance that will be in the set after the function completes.</param>
    /// <returns>True, if the element was present before. False if the element was added.</returns>
    [WriteAccess]
    public bool Update(TElement element, Func<TElement, TElement> updateFunc)
    {
        bool result = Remove(element);
        _ = Add(updateFunc(element));
        return result;
    }

    /// <summary>
    /// Helper function that allows to safely update immutable, indexed keys within <paramref name="updateFunc"/>.
    /// </summary>
    /// <param name="element">The element to update. Is passed to <paramref name="updateFunc"/>.</param>
    /// <param name="state">User defined state that is passed to <paramref name="updateFunc"/>.</param>
    /// <param name="updateFunc">The update function, can return the same or a new instance that will be in the set after the function completes.</param>
    /// <returns>True, if the element was present before. False if the element was added.</returns>
    [WriteAccess]
    public bool Update<TState>(TElement element, TState state, Func<TElement, TState, TElement> updateFunc)
    {
        bool result = Remove(element);
        _ = Add(updateFunc(element, state));
        return result;
    }

    internal void AddIndex<TIndexKey, TIndex>(Func<TElement, TIndexKey> indexAccessor, TIndex index)
#if NET9_0_OR_GREATER
        where TIndexKey : notnull, allows ref struct
#else
        where TIndexKey : notnull
#endif
        where TIndex : TypedIndex<TElement, TIndexKey>
    {
        AddIndex(new SingleKeyIndexWriter<TElement, TIndexKey, TIndex>(indexAccessor, index), index);
    }

    internal void AddIndex<TIndexKey, TIndex>(Func<TElement, IEnumerable<TIndexKey>> indexAccessor, TIndex index)
#if NET9_0_OR_GREATER
        where TIndexKey : notnull, allows ref struct
#else
        where TIndexKey : notnull
#endif
        where TIndex : TypedIndex<TElement, TIndexKey>
    {
        AddIndex(new MultiKeyIndexWriter<TElement, TIndexKey, TIndex>(indexAccessor, index), index);
    }

    internal void AddIndex(IndexWriter<TElement> writer, Index<TElement> index)
    {
        ThrowIfNonEmpty();

        _indices = _indices.Append(new KeyValuePair<string, Index<TElement>>(index.Name, index)).ToFrozenDictionary();
        _indexWriters = _indexWriters.Append(new KeyValuePair<string, IndexWriter<TElement>>(index.Name, writer)).ToFrozenDictionary();
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
/// Additionally provides convenience access to a "primary key" unique index.
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
    /// <param name="keyComparer">A key comparer used to compare primary keys. If null is passed, the default comparer is used.</param>
    protected internal IndexedSet(Func<TElement, TPrimaryKey> primaryKeyAccessor, IEqualityComparer<TPrimaryKey>? keyComparer = null, [CallerArgumentExpression(nameof(primaryKeyAccessor))] string primaryKeyIndexName = "")
    {
        _primaryKeyAccessor = primaryKeyAccessor;
        _primaryKeyIndexName = primaryKeyIndexName;

        AddIndex(_primaryKeyAccessor, new UniqueIndex<TElement, TPrimaryKey>(keyComparer ?? EqualityComparer<TPrimaryKey>.Default, primaryKeyIndexName));
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// Short-hand for <see cref="Single(TPrimaryKey)"/>.
    /// </summary>
    public TElement this[TPrimaryKey key] => Single(key);

    /// <summary>
    /// Tries to get an element associated to the given primary key
    /// </summary>
    /// <param name="key">The primary key to obtain the item for</param>
    /// <param name="result">The found element if the method returns true, otherwise null.</param>
    /// <returns>True if an element is found, otherwise false.</returns>
    [ReadAccess]
    public bool TryGetSingle(TPrimaryKey key, [NotNullWhen(true)] out TElement? result)
    {
        return TryGetSingle(_primaryKeyAccessor, key, out result, _primaryKeyIndexName);
    }

    /// <summary>
    /// Attempts to remove an item with the given primary key and returns true, if one was found and removed. Otherwise, false.
    /// </summary>
    [WriteAccess]
    public bool Remove(TPrimaryKey key)
    {
        return TryGetSingle(_primaryKeyAccessor, key, out TElement? result, _primaryKeyIndexName) && Remove(result);
    }

    /// <summary>
    /// Returns the element associated to the given primary key.
    /// </summary>
    /// <param name="key">The primary key to obtain the item for</param>
    [ReadAccess]
    public TElement Single(TPrimaryKey key)
    {
        return Single(_primaryKeyAccessor, key, _primaryKeyIndexName);
    }

    /// <summary>
    /// Returns true if an element with the given primary key is present in the current set, otherwise, false.
    /// </summary>
    [ReadAccess]
    public bool Contains(TPrimaryKey key)
    {
        return Where(_primaryKeyAccessor, key, _primaryKeyIndexName).Any();
    }
}
