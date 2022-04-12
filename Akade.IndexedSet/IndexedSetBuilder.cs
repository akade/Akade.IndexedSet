using Akade.IndexedSet.Indices;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet;

/// <summary>
/// Helper class to support type inference for element and primary key type for <see cref="IndexedSetBuilder{TPrimaryKey, TElement}"/>
/// </summary>
public static class IndexedSetBuilder
{
    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> Create<TPrimaryKey, TElement>(IEnumerable<TElement> initialContent, Func<TElement, TPrimaryKey> primaryKeyAccessor)
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, initialContent);
    }

    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> ToIndexedSet<TPrimaryKey, TElement>(this IEnumerable<TElement> initialContent, Func<TElement, TPrimaryKey> primaryKeyAccessor)
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, initialContent);
    }
}

/// <summary>
/// Helper class to support type inference for the primary key type for <see cref="IndexedSetBuilder{TPrimaryKey, TElement}"/>
/// </summary>
public static class IndexedSetBuilder<TElement>
{
    /// <summary>
    /// Creates a new builder instance. 
    /// Use <see cref="IndexedSetBuilder.Create{TPrimaryKey, TElement}(IEnumerable{TElement}, Func{TElement, TPrimaryKey})" /> if you
    /// want to include initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> Create<TPrimaryKey>(Func<TElement, TPrimaryKey> primaryKeyAccessor)
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, null);
    }
}

/// <summary>
/// Helper class to create <see cref="IndexedSet{TPrimaryKey, TElement}"/> with a fluent syntax.
/// Use <see cref="IndexedSetBuilder{TElement}.Create{TPrimaryKey}(Func{TElement, TPrimaryKey})" /> to obtain a builder syntax.
/// </summary>
public class IndexedSetBuilder<TPrimaryKey, TElement> where TPrimaryKey : notnull
{
    private readonly IndexedSet<TPrimaryKey, TElement> _result;
    private readonly IEnumerable<TElement>? _initialContent;

    internal IndexedSetBuilder(Func<TElement, TPrimaryKey> primaryKeyAccessor, IEnumerable<TElement>? initialContent)
    {
        _result = new(primaryKeyAccessor);
        _initialContent = initialContent;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TPrimaryKey, TElement}"/> to have an unique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternativly, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public IndexedSetBuilder<TPrimaryKey, TElement> WithUniqueIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new UniqueIndex<TPrimaryKey, TElement, TIndexKey>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TPrimaryKey, TElement}"/> to have a nonunique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternativly, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public IndexedSetBuilder<TPrimaryKey, TElement> WithIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new NonUniqueIndex<TPrimaryKey, TElement, TIndexKey>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TPrimaryKey, TElement}"/> to have a nonunique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternativly, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public IndexedSetBuilder<TPrimaryKey, TElement> WithIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new MultiValueIndex<TPrimaryKey, TElement, TIndexKey>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TPrimaryKey, TElement}"/> to have a nonunique index based on a secondary key that supports range queries.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternativly, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public IndexedSetBuilder<TPrimaryKey, TElement> WithRangeIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new RangeIndex<TPrimaryKey, TElement, TIndexKey>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="IndexedSet{TPrimaryKey, TElement}"/>
    /// </summary>
    public IndexedSet<TPrimaryKey, TElement> Build()
    {
        if(_initialContent is not null)
        {
            _result.AddRange(_initialContent);
        }

        return _result;
    }
}
