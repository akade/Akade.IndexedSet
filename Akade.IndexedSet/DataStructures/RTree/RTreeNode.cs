#if NET9_0_OR_GREATER
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;

internal abstract class Node<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    internal abstract AABB<TValue> GetAABB(Func<TElement, AABB<TValue>> getAABB);
}

internal sealed class ParentNode<TElement, TValue> : Node<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
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

    internal void MergeAABB(AABB<TValue> other)
    {
        if (IsEmptyAABB)
        {
            var backingMemory = new TValue[other.Min.Length * 2];

            Min = backingMemory.AsMemory(0, other.Min.Length);
            Max = backingMemory.AsMemory(other.Min.Length, other.Max.Length);

            other.Min.CopyTo(Min.Span);
            other.Max.CopyTo(Max.Span);
            return;
        }

        Span<TValue> buffer = stackalloc TValue[Min.Length];

        TensorPrimitives.Min(Min.Span, other.Min, buffer);
        buffer.CopyTo(Min.Span);

        TensorPrimitives.Max(Max.Span, other.Max, buffer);
        buffer.CopyTo(Max.Span);
    }

    internal void RecalculateAABB(Func<TElement, AABB<TValue>> getAABB)
    {
        Span<Node<TElement, TValue>> childSpan = CollectionsMarshal.AsSpan(Children);

        AABB<TValue> firstChild = childSpan[0].GetAABB(getAABB);
        firstChild.Min.CopyTo(Min.Span);
        firstChild.Max.CopyTo(Max.Span);

        for (int i = 1; i < childSpan.Length; i++)
        {
            MergeAABB(Children[i].GetAABB(getAABB));
        }
    }

    public override string ToString()
    {
        return $"ParentNode: {Children.Count} children, AABB: {GetAABB(null!).ToString()}";
    }
}

internal sealed class LeafNode<TElement, TValue>(TElement element) : Node<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    public TElement Element { get; } = element;

    internal override AABB<TValue> GetAABB(Func<TElement, AABB<TValue>> getAABB)
    {
        return getAABB(Element);
    }

    public override string ToString()
    {
        return $"LeafNode: {Element}";
    }
}

#endif