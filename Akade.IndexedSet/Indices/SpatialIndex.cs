using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Extensions;
using Akade.IndexedSet.Utils;
using System.Numerics;

namespace Akade.IndexedSet.Indices;

internal sealed class SpatialIndex<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>(Func<TElement, TEnvelope> getAABB, int dimensions, RTreeSettings settings, string name) : TypedIndex<TElement, TPoint>(name)
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

    internal override void Add(TPoint key, TElement value)
    {
        _tree.Insert(value);
    }

    internal override void AddRange(IKeyValueEnumerator<TPoint, TElement> elementsToAdd)
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

    internal override void Remove(TPoint key, TElement value)
    {
        _ = _tree.Remove(value);
    }

    internal override TElement Single(TPoint indexKey)
    {
        TEnvelope envelope = TEnvelopeMath.CreateFromPoint(indexKey);
        return _tree.IntersectWith(envelope).SingleThrowingKeyNotFoundException();
    }

    internal override bool TryGetSingle(TPoint indexKey, out TElement? element)
    {
        TEnvelope envelope = TEnvelopeMath.CreateFromPoint(indexKey);
        IEnumerable<TElement> allMatches = _tree.IntersectWith(envelope);
        IEnumerator<TElement> enumerator = allMatches.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            element = default;
            return false;
        }

        element = enumerator.Current;
        return !enumerator.MoveNext();
    }

    internal override IEnumerable<TElement> Where(TPoint indexKey)
    {
        TEnvelope envelope = TEnvelopeMath.CreateFromPoint(indexKey);
        return _tree.IntersectWith(envelope);
    }

    internal override IEnumerable<TElement> Range(TPoint start, TPoint end, bool inclusiveStart, bool inclusiveEnd)
    {
        TEnvelope bounds = TEnvelopeMath.Create(start, end);
        // TODO: inclusivity is not handled yet
        return _tree.IntersectWith(bounds);
    }

    internal override IEnumerable<TElement> NearestNeighbors(TPoint indexKey)
    {
        TEnvelope envelope = TEnvelopeMath.CreateFromPoint(indexKey);
        return _tree.GetNearestNeighbours(indexKey).Select(x => x.element);
    }
}
