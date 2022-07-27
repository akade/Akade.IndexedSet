using Akade.IndexedSet.Concurrency;
using Akade.IndexedSet.Indices;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet;

/// <summary>
/// Helper class to support type inference for element <see cref="IndexedSetBuilder{TElement}"/> and primary key type for <see cref="IndexedSetBuilder{TPrimaryKey, TElement}"/>
/// </summary>
public static class IndexedSetBuilder
{
    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> Create<TPrimaryKey, TElement>(
        IEnumerable<TElement> initialContent,
        Func<TElement, TPrimaryKey> primaryKeyAccessor,
        [CallerArgumentExpression("primaryKeyAccessor")] string primaryKeyIndexName = "")
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, initialContent, primaryKeyIndexName);
    }

    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> ToIndexedSet<TPrimaryKey, TElement>(
        this IEnumerable<TElement> initialContent,
        Func<TElement, TPrimaryKey> primaryKeyAccessor,
        [CallerArgumentExpression("primaryKeyAccessor")] string primaryKeyIndexName = "")
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, initialContent, primaryKeyIndexName);
    }

    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TElement> Create<TElement>(IEnumerable<TElement> initialContent)
    {
        return new IndexedSetBuilder<TElement>(null, initialContent);
    }

    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TElement> ToIndexedSet<TElement>(this IEnumerable<TElement> initialContent)
    {
        return new IndexedSetBuilder<TElement>(null, initialContent);
    }
}

/// <summary>
/// Helper class to support type inference for the primary key type for <see cref="IndexedSetBuilder{TPrimaryKey, TElement}"/>
/// </summary>
public class IndexedSetBuilder<TElement>
{
    private readonly IndexedSet<TElement> _result;
    private readonly IEnumerable<TElement>? _initialContent;

    /// <summary>
    /// Creates a new builder instance. 
    /// Use <see cref="IndexedSetBuilder.Create{TPrimaryKey, TElement}(IEnumerable{TElement}, Func{TElement, TPrimaryKey}, string)" /> if you
    /// want to include initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> Create<TPrimaryKey>(Func<TElement, TPrimaryKey> primaryKeyAccessor, [CallerArgumentExpression("primaryKeyAccessor")] string primaryKeyIndexName = "")
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, null, primaryKeyIndexName);
    }

    /// <summary>
    /// Creates a new builder instance. 
    /// Use <see cref="IndexedSetBuilder.Create{TElement}(IEnumerable{TElement})" /> if you
    /// want to include initial content
    /// </summary>
    public static IndexedSetBuilder<TElement> Create()
    {
        return new IndexedSetBuilder<TElement>(null, null);
    }

    internal IndexedSetBuilder(IndexedSet<TElement>? indexedSet, IEnumerable<TElement>? initialContent)
    {
        _result = indexedSet ?? new();
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
    public virtual IndexedSetBuilder<TElement> WithUniqueIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new UniqueIndex<TElement, TIndexKey>(keyAccessor, indexName));

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
    public virtual IndexedSetBuilder<TElement> WithIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new NonUniqueIndex<TElement, TIndexKey>(keyAccessor, indexName));

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
    public virtual IndexedSetBuilder<TElement> WithIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new MultiValueIndex<TElement, TIndexKey>(keyAccessor, indexName));

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
    public virtual IndexedSetBuilder<TElement> WithRangeIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new RangeIndex<TElement, TIndexKey>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TPrimaryKey, TElement}"/> to have a full text index based on a secondary key that 
    /// supports fuzzy search on string startswith/contains queries. The secondary key can be any expression that does not change while 
    /// the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.StringProp1. Alternativly, you can also always use the same method from a static class.
    /// </summary>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithFullTextIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new FullTextIndex<TElement>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TPrimaryKey, TElement}"/> to have a prefix index based on a secondary key that 
    /// supports fuzzy search on string startswith queries. The secondary key can be any expression that does not change while 
    /// the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.StringProp1. Alternativly, you can also always use the same method from a static class.
    /// </summary>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithPrefixIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        if (indexName is null)
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        _result.AddIndex(new PrefixIndex<TElement>(keyAccessor, indexName));

        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="IndexedSet{TPrimaryKey, TElement}"/>
    /// </summary>
    public virtual IndexedSet<TElement> Build()
    {
        if (_initialContent is not null)
        {
            _ = _result.AddRange(_initialContent);
        }

        return _result;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="IndexedSet{TPrimaryKey, TElement}"/>
    /// </summary>
    public virtual ConcurrentIndexedSet<TElement> BuildConcurrent()
    {
        return new(Build());
    }
}

/// <summary>
/// Helper class to create <see cref="IndexedSet{TPrimaryKey, TElement}"/> with a fluent syntax.
/// Use <see cref="IndexedSetBuilder{TElement}" /> and <see cref="IndexedSetBuilder{TPrimaryKey, TElement}"/> to obtain a builder.
/// </summary>
public class IndexedSetBuilder<TPrimaryKey, TElement> : IndexedSetBuilder<TElement>
    where TPrimaryKey : notnull
{

    internal IndexedSetBuilder(Func<TElement, TPrimaryKey> primaryKeyAccessor, IEnumerable<TElement>? initialContent, string primaryKeyIndexName) : base(new IndexedSet<TPrimaryKey, TElement>(primaryKeyAccessor, primaryKeyIndexName), initialContent)
    {
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        _ = base.WithIndex(keyAccessor, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        _ = base.WithIndex(keyAccessor, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithRangeIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        _ = base.WithRangeIndex(keyAccessor, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithUniqueIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        _ = base.WithUniqueIndex(keyAccessor, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithFullTextIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        _ = base.WithFullTextIndex(keyAccessor, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithPrefixIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, [CallerArgumentExpression("keyAccessor")] string? indexName = null)
    {
        _ = base.WithPrefixIndex(keyAccessor, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSet<TPrimaryKey, TElement> Build()
    {
        return (IndexedSet<TPrimaryKey, TElement>)base.Build();
    }

    /// <summary>
    /// Builds and returns the configured <see cref="IndexedSet{TPrimaryKey, TElement}"/>
    /// </summary>
    public override ConcurrentIndexedSet<TPrimaryKey, TElement> BuildConcurrent()
    {
        return new(Build());
    }

}
