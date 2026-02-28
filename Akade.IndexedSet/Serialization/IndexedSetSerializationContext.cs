using System.Runtime.InteropServices;

namespace Akade.IndexedSet.Serialization;

public interface ISerializationAdapter
{
    ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
        where TElement : notnull;
    public ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken);
}


internal record IndexedSetSerializationContext<TElement>(ISerializationAdapter Serializer)
    where TElement : notnull
{
    private readonly Dictionary<TElement, int> _elementIds = new();

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
