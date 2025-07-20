#if NET9_0_OR_GREATER
using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Extensions;
using Akade.IndexedSet.Utils;
using System.Numerics;

namespace Akade.IndexedSet.Indices;

internal sealed class SpatialIndex<TElement, TValue>(Func<TElement, AABB<TValue>> getAABB, int dimensions, RTreeSettings settings, string name) : TypedIndex<TElement, AABB<TValue>>(name)
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    private readonly RTree<TElement, TValue> _tree = new(getAABB, dimensions, settings);

    public override void Clear()
    {
        _tree.Clear();
    }

    internal override void Add(AABB<TValue> key, TElement value)
    {
        _tree.Insert(value);
    }

    internal override void AddRange(IKeyValueEnumerator<AABB<TValue>, TElement> elementsToAdd)
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

    internal override void Remove(AABB<TValue> key, TElement value)
    {
        _ = _tree.Remove(value);
    }

    internal override TElement Single(AABB<TValue> indexKey)
    {
        return _tree.IntersectWith(indexKey).SingleThrowingKeyNotFoundException();
    }

    internal override bool TryGetSingle(AABB<TValue> indexKey, out TElement? element)
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

    internal override IEnumerable<TElement> Where(AABB<TValue> indexKey)
    {
        return _tree.IntersectWith(indexKey);
    }
}

#endif