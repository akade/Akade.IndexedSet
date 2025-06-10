#if NET9_0_OR_GREATER
using System.Numerics;
using System.Numerics.Tensors;

namespace Akade.IndexedSet.DataStructures.RTree;

internal abstract class Node<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>
{
    internal abstract AABB<TValue> GetAABB(Func<TElement, AABB<TValue>> getAABB);
}

internal sealed class ParentNode<TElement, TValue> : Node<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>
{
    public List<Node<TElement, TValue>> Children = [];

    public ParentNode() 
    {
        
    }

    public ParentNode(Span<Node<TElement, TValue>> span, Func<TElement, AABB<TValue>> getAABB)
    {
        Children.AddRange(span);
        RecalculateAABB(getAABB);
    }

    public Memory<TValue> Min { get; private set; }
    public Memory<TValue> Max { get; private set; }

    internal override AABB<TValue> GetAABB(Func<TElement, AABB<TValue>> getAABB)
    {
        return new AABB<TValue>(Min.Span, Max.Span);
    }

    public bool IsEmptyAABB => Min.Length == 0;

    internal void MergeAABB(AABB<TValue> aABB)
    {
        if (IsEmptyAABB)
        {
            var backingMemory = new TValue[aABB.Min.Length * 2];

            Min = backingMemory.AsMemory(0, aABB.Min.Length);
            Max = backingMemory.AsMemory(aABB.Min.Length, aABB.Max.Length);

            aABB.Min.CopyTo(Min.Span);
            aABB.Max.CopyTo(Max.Span);
            return;
        }

        Span<TValue> buffer = stackalloc TValue[Min.Length];

        TensorPrimitives.Min(Min.Span, aABB.Min, buffer);
        buffer.CopyTo(Min.Span);

        TensorPrimitives.Max(Max.Span, aABB.Max, buffer);
        buffer.CopyTo(Max.Span);
    }

    internal void RecalculateAABB(Func<TElement, AABB<TValue>> getAABB)
    {
        AABB<TValue> firstChild = Children[0].GetAABB(getAABB);
        firstChild.Min.CopyTo(Min.Span);
        firstChild.Max.CopyTo(Max.Span);

        for (int i = 1; i < Children.Count; i++)
        {
            MergeAABB(Children[i].GetAABB(getAABB));
        }
    }
}

internal sealed class LeafNode<TElement, TValue>(TElement element) : Node<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>
{
    public TElement element { get; } = element;

    internal override AABB<TValue> GetAABB(Func<TElement, AABB<TValue>> getAABB)
    {
        return getAABB(element);
    }
}

#endif