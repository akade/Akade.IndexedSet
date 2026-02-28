using Akade.IndexedSet.Serialization;

namespace Akade.IndexedSet.Indices;

/// <summary>
/// Non-generic on the index key to have a strongly typed base class for an index
/// </summary>
internal abstract class Index<TElement>(string name)
    where TElement : notnull
{
    public string Name { get; } = name;

    public abstract void Clear();

    public abstract int IndexTypeNumber { get; }

    internal virtual bool SupportsSerialization => false;

    internal virtual ValueTask SerializeAsync(IndexedSetSerializationContext<TElement> context, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotSupportedException($"Serialization is not supported on {GetType().Name}-indices.");
    }


    internal virtual ValueTask DeserializeAsync(IndexedSetSerializationContext<TElement> context, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotSupportedException($"Deserialization is not supported on {GetType().Name}-indices.");
    }
}