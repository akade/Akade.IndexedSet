using System.Runtime.InteropServices;

namespace Akade.IndexedSet.Serialization;

/// <summary>
/// Defines an interface for serialization adapters that can serialize and deserialize objects to and from streams.
/// Note that it needs to support general serialization and deserialization, i.e. not only the element type of the IndexedSet but also all key types.
/// </summary>
public interface ISerializationAdapter
{
    /// <summary>
    /// Deserializes an object of type TElement from the provided stream asynchronously.
    /// </summary>
    /// <typeparam name="TElement">The type of the object to deserialize.</typeparam>
    /// <param name="source">The stream to deserialize the object from. The implementation does not own the stream and should not close it.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object.</returns>
    ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
        where TElement : notnull;
   
    /// <summary>
    /// Serializes an object of type T to the provided stream asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="element">The object to serialize.</param>
    /// <param name="stream">The stream to serialize the object to. The implementation does not own the stream and should not close it.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken);
}


internal record IndexedSetSerializationContext<TElement>(ISerializationAdapter Serializer, int NumberOfElements)
    where TElement : notnull
{
    private readonly Dictionary<TElement, int> _elementIds = new(NumberOfElements);

    public int GetOrAddElementId(TElement element)
    {
        ref int id = ref CollectionsMarshal.GetValueRefOrAddDefault(_elementIds, element, out bool exists);

        if (!exists)
        {
            id = _elementIds.Count - 1; // 0-based indexing, so the id of the newly added element is count - 1
        }

        return id;
    }

    internal void SetElementId(TElement element, int id)
    {
        _elementIds[element] = id;
    }
}
