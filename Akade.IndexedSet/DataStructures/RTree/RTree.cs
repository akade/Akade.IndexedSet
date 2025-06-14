#if NET9_0_OR_GREATER
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;

/// <summary>
/// Implemented based on https://github.com/georust/rstar/blob/master/rstar/src/algorithm/rstar.rs (MIT/Apache-2.0 License).
/// Many thanks for the legwork
/// </summary>

// TODO: explore on how to specialize with minimal code duplication for Vector2 & Vector3
internal sealed partial class RTree<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    private readonly Func<TElement, AABB<TValue>> _getAABB;
    private readonly int _dimensions;
    private readonly RTreeSettings _settings;
    private ParentNode<TElement, TValue> _root = new();

    internal RTree(Func<TElement, AABB<TValue>> getAABB, int dimensions, RTreeSettings settings)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimensions, 1);
        settings.Validate();
        _getAABB = getAABB;
        _dimensions = dimensions;
        _settings = settings;
    }

   
}


#endif 
