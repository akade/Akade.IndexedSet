using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Extensions;
using Akade.IndexedSet.Utils;
using System.Numerics;

namespace Akade.IndexedSet.Indices;

internal sealed class SpatialIndex<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>(Func<TElement, TEnvelope> getAABB, int dimensions, RTreeSettings settings, string name) : TypedIndex<TElement, TEnvelope>(name)
    where TPoint : struct
    where TEnvelope : struct 
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
    where TEnvelopeMath : struct, IEnvelopeMath<TPoint, TEnvelope, TValue>
{
    private readonly RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath> _tree = new(getAABB, dimensions, settings);

    public override void Clear()
    {
        _tree.Clear();
    }

    internal override void Add(TEnvelope key, TElement value)
    {
        _tree.Insert(value);
    }

    internal override void AddRange(IKeyValueEnumerator<TEnvelope, TElement> elementsToAdd)
    {
        if (_tree.Count == 0)
        {
            _tree.BulkLoad(elementsToAdd.GetRawValues());
        }
        else
        {
            base.AddRange(elementsToAdd);
        }
    }

    internal override void Remove(TEnvelope key, TElement value)
    {
        _ = _tree.Remove(value);
    }

    internal override TElement Single(TEnvelope indexKey)
    {
        return _tree.IntersectWith(indexKey).SingleThrowingKeyNotFoundException();
    }

    internal override bool TryGetSingle(TEnvelope indexKey, out TElement? element)
    {
        IEnumerable<TElement> allMatches = _tree.IntersectWith(indexKey);
        IEnumerator<TElement> enumerator = allMatches.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            element = default;
            return false;
        }

        element = enumerator.Current;
        return !enumerator.MoveNext();
    }

    internal override IEnumerable<TElement> Where(TEnvelope indexKey)
    {
        return _tree.IntersectWith(indexKey);
    }
}
