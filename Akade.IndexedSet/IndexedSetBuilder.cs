// Ignore Spelling: Accessor

using Akade.IndexedSet.Concurrency;
using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Indices;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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
        [CallerArgumentExpression(nameof(primaryKeyAccessor))] string primaryKeyIndexName = "")
        where TElement : notnull
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
        [CallerArgumentExpression(nameof(primaryKeyAccessor))] string primaryKeyIndexName = "")
        where TElement : notnull
        where TPrimaryKey : notnull
    {
        return new IndexedSetBuilder<TPrimaryKey, TElement>(primaryKeyAccessor, initialContent, primaryKeyIndexName);
    }

    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TElement> Create<TElement>(IEnumerable<TElement> initialContent)
        where TElement : notnull
    {
        return new IndexedSetBuilder<TElement>(null, initialContent);
    }

    /// <summary>
    /// Creates a new builder instance with the given initial content
    /// </summary>
    public static IndexedSetBuilder<TElement> ToIndexedSet<TElement>(this IEnumerable<TElement> initialContent)
        where TElement : notnull
    {
        return new IndexedSetBuilder<TElement>(null, initialContent);
    }
}

/// <summary>
/// Helper class to support type inference for the primary key type for <see cref="IndexedSetBuilder{TPrimaryKey, TElement}"/>
/// </summary>
public class IndexedSetBuilder<TElement>
    where TElement : notnull
{
    private readonly IndexedSet<TElement> _result;
    private readonly IEnumerable<TElement>? _initialContent;

    /// <summary>
    /// Creates a new builder instance. 
    /// Use <see cref="IndexedSetBuilder.Create{TPrimaryKey, TElement}(IEnumerable{TElement}, Func{TElement, TPrimaryKey}, string)" /> if you
    /// want to include initial content
    /// </summary>
    public static IndexedSetBuilder<TPrimaryKey, TElement> Create<TPrimaryKey>(Func<TElement, TPrimaryKey> primaryKeyAccessor, [CallerArgumentExpression(nameof(primaryKeyAccessor))] string primaryKeyIndexName = "")
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
    /// Configures the <see cref="IndexedSet{TElement}"/> to have an unique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="keyComparer">The comparer to use for the index. If not specified, the default comparer for <typeparamref name="TIndexKey"/> is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithUniqueIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);
        keyComparer ??= EqualityComparer<TIndexKey>.Default;

        _result.AddIndex(keyAccessor, new UniqueIndex<TElement, TIndexKey>(keyComparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have an unique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="keyComparer">The comparer to use for the index. If not specified, the default comparer for <typeparamref name="TIndexKey"/> is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithUniqueIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);
        keyComparer ??= EqualityComparer<TIndexKey>.Default;

        _result.AddIndex(keyAccessor, new UniqueIndex<TElement, TIndexKey>(keyComparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a nonunique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="keyComparer">The comparer to use for the index. If not specified, the default comparer for <typeparamref name="TIndexKey"/> is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);
        keyComparer ??= EqualityComparer<TIndexKey>.Default;

        _result.AddIndex(keyAccessor, new NonUniqueIndex<TElement, TIndexKey>(keyComparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a nonunique index based on a secondary key.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="keyComparer">The comparer to use for the index. If not specified, the default comparer for <typeparamref name="TIndexKey"/> is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);
        keyComparer ??= EqualityComparer<TIndexKey>.Default;

        _result.AddIndex(keyAccessor, new NonUniqueIndex<TElement, TIndexKey>(keyComparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a nonunique index based on a secondary key that supports range queries.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="keyComparer">The comparer to use for the index. If not specified, the default comparer for <typeparamref name="TIndexKey"/> is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithRangeIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, IComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);
        keyComparer ??= Comparer<TIndexKey>.Default;

        _result.AddIndex(keyAccessor, new RangeIndex<TElement, TIndexKey>(keyComparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a nonunique index based on a secondary key that supports range queries.
    /// The secondary key can be any expression that does not change while the element is within the indexed set, even
    /// tuples or calculated expressions. The name of the index is based on the string representation of the expression
    /// and passed by the compiler to <paramref name="indexName"/>. The convention is to always use x as a lambda parameter:
    /// x => (x.Prop1, x.Prop2, x.Prop3). Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the key within the index</typeparam>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="keyComparer">The comparer to use for the index. If not specified, the default comparer for <typeparamref name="TIndexKey"/> is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithRangeIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, IComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TIndexKey : notnull
    {
        ArgumentNullException.ThrowIfNull(indexName);
        keyComparer ??= Comparer<TIndexKey>.Default;

        _result.AddIndex(keyAccessor, new MultiRangeIndex<TElement, TIndexKey>(keyComparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a full text index based on a secondary key that 
    /// supports fuzzy search on string startswith/contains queries. The secondary key can be any expression that does not change while 
    /// the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.StringProp1. Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="comparer">The comparer to use for the index. If not specified, the default comparer is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithFullTextIndex(Func<TElement, string> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(indexName);
        comparer ??= EqualityComparer<char>.Default;

        _result.AddIndex(keyAccessor, new FullTextIndex<TElement>(keyAccessor, comparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a full text index based on a secondary key that 
    /// supports fuzzy search on string startswith/contains queries. The secondary key can be any expression that does not change while 
    /// the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.StringProp1. Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="comparer">The comparer to use for the index. If not specified, the default comparer is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public virtual IndexedSetBuilder<TElement> WithFullTextIndex(Func<TElement, IEnumerable<string>> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(indexName);
        comparer ??= EqualityComparer<char>.Default;

        _result.AddIndex(keyAccessor, new FullTextIndex<TElement>(keyAccessor, comparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a prefix index based on a secondary key that 
    /// supports fuzzy search on string startswith queries. The secondary key can be any expression that does not change while 
    /// the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.StringProp1. Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="comparer">The comparer to use for the index. If not specified, the default comparer is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    public virtual IndexedSetBuilder<TElement> WithPrefixIndex(Func<TElement, string> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(indexName);
        comparer ??= EqualityComparer<char>.Default;

        _result.AddIndex(keyAccessor, new PrefixIndex<TElement>(comparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a prefix index based on a secondary key that 
    /// supports fuzzy search on string startswith queries. The secondary key can be any expression that does not change while 
    /// the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.StringProp1. Alternatively, you can also always use the same method from a static class.
    /// </summary>
    /// <param name="keyAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. 
    /// Hence, the convention is to always use x as an identifier in case a lambda expression is used.</param>
    /// <param name="comparer">The comparer to use for the index. If not specified, the default comparer is used.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="keyAccessor"/> is automatically passed by the compiler.</param>
    /// <returns>The instance on which this method is called is returned to support the fluent syntax.</returns>
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public virtual IndexedSetBuilder<TElement> WithPrefixIndex(Func<TElement, IEnumerable<string>> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(indexName);
        comparer ??= EqualityComparer<char>.Default;

        _result.AddIndex(keyAccessor, new PrefixIndex<TElement>(comparer, indexName));

        return this;
    }

    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a spatial (point) index based on a secondary key that 
    /// supports fast spatial queries (range and nearest neighbor search). Currently supported types: <see cref="Vector2"/> and <see cref="Vector3"/>
    /// 
    /// The secondary key can be any expression that does not change while the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.Point. Alternatively, you can also always use the same method from a static class.
    /// </summary>
    public virtual IndexedSetBuilder<TElement> WithSpatialIndex<TPoint>(Func<TElement, TPoint> keyAccessor, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
        where TPoint : struct
    {
        ArgumentNullException.ThrowIfNull(indexName);

        if (typeof(TPoint) == typeof(Vector2))
        {
            var castedAccessorVec2 = (Func<TElement, Vector2>)(object)keyAccessor;

            VecRec2 vecrec2Accessor(TElement element) => VecRec2.CreateFromPoint(castedAccessorVec2(element));
            SpatialIndex<TElement, Vector2, VecRec2, float, Vector2Math> spatialIndex = new(vecrec2Accessor, 2, RTreeSettings.Default, indexName);
            _result.AddIndex(castedAccessorVec2, spatialIndex);
        }
        else if (typeof(TPoint) == typeof(Vector3))
        {
            var castedAccessorVec3 = (Func<TElement, Vector3>)(object)keyAccessor;

            VecRec3 vecrec3Accessor(TElement element) => VecRec3.CreateFromPoint(castedAccessorVec3(element));
            SpatialIndex<TElement, Vector3, VecRec3, float, Vector3Math> spatialIndex = new(vecrec3Accessor, 3, RTreeSettings.Default, indexName);
            _result.AddIndex(castedAccessorVec3, spatialIndex);

        }
        else
        {
            throw new InvalidOperationException($"Currently only {nameof(Vector2)} and {nameof(Vector3)} are supported, {typeof(TPoint).Name} is not.");
        }

        return this;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Configures the <see cref="IndexedSet{TElement}"/> to have a vector index based on a secondary key that 
    /// supports approximate nearest neighbor queries using cosine similarity. Currently supports <see cref="ReadOnlySpan{T}"/> with <see cref="float" />.
    /// 
    /// The secondary key can be any expression that does not change while the element is within the indexed set. The name of the index is based on the 
    /// string representation of the expression and passed by the compiler to <paramref name="indexName"/>. 
    /// The convention is to always use x as a lambda parameter: x => x.Embedding. Alternatively, you can also always use the same method from a static class.
    /// </summary>
    public virtual IndexedSetBuilder<TElement> WithVectorIndex(Func<TElement, ReadOnlySpan<float>> keyAccessor, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(indexName);
        _result.AddIndex(keyAccessor, new VectorIndex<TElement>(keyAccessor, indexName));
        return this;
    }
#endif

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
    where TElement : notnull
    where TPrimaryKey : notnull
{

    internal IndexedSetBuilder(Func<TElement, TPrimaryKey> primaryKeyAccessor, IEnumerable<TElement>? initialContent, string primaryKeyIndexName, IEqualityComparer<TPrimaryKey>? keyComparer = null)
        : base(new IndexedSet<TPrimaryKey, TElement>(primaryKeyAccessor, keyComparer, primaryKeyIndexName), initialContent)
    {
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithIndex(keyAccessor, keyComparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithIndex(keyAccessor, keyComparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithRangeIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, IComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithRangeIndex(keyAccessor, keyComparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithRangeIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, IComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithRangeIndex(keyAccessor, keyComparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithUniqueIndex<TIndexKey>(Func<TElement, TIndexKey> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithUniqueIndex(keyAccessor, keyComparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithUniqueIndex<TIndexKey>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, IEqualityComparer<TIndexKey>? keyComparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithUniqueIndex(keyAccessor, keyComparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithFullTextIndex(Func<TElement, string> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithFullTextIndex(keyAccessor, comparer, indexName);
        return this;
    }

    /// <inheritdoc />
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public override IndexedSetBuilder<TElement> WithFullTextIndex(Func<TElement, IEnumerable<string>> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithFullTextIndex(keyAccessor, comparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithPrefixIndex(Func<TElement, string> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithPrefixIndex(keyAccessor, comparer, indexName);
        return this;
    }

    /// <inheritdoc />
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithPrefixIndex(Func<TElement, IEnumerable<string>> keyAccessor, IEqualityComparer<char>? comparer = null, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithPrefixIndex(keyAccessor, comparer, indexName);
        return this;
    }

    /// <inheritdoc />
    public override IndexedSetBuilder<TPrimaryKey, TElement> WithSpatialIndex<TPoint>(Func<TElement, TPoint> keyAccessor, [CallerArgumentExpression(nameof(keyAccessor))] string? indexName = null)
    {
        _ = base.WithSpatialIndex(keyAccessor, indexName);
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
