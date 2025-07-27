#if NET9_0_OR_GREATER
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;

internal sealed partial class RTree<TElement, TEnvelope, TValue, TMemoryEnvelope, TEnvelopeMath>
{

    internal abstract class Node
    {
        internal abstract TEnvelope GetEnvelope(Func<TElement, TEnvelope> getAABB);
    }

    internal sealed class ParentNode : Node
    {
        public List<Node> Children = [];

        public ParentNode()
        {

        }

        public ParentNode(Span<Node> span, Func<TElement, TEnvelope> getAABB)
        {
            Children.AddRange(span);
            RecalculateAABB(getAABB);
        }

        public TMemoryEnvelope Envelope { get; set; } = TEnvelopeMath.GetEmptyMemory();

        internal override TEnvelope GetEnvelope(Func<TElement, TEnvelope> getAABB)
        {
            return TEnvelopeMath.AsEnvelope(Envelope);
        }

        public bool IsEmptyEnvelope => TEnvelopeMath.IsEmpty(Envelope);

        internal void MergeEnvelope(TEnvelope other)
        {
            if (IsEmptyEnvelope)
            {
                InitMemory(other);
                return;
            }

            TEnvelopeMath.Merge(Envelope, other, Envelope);
        }

        private void InitMemory(TEnvelope aabb)
        {
            if (IsEmptyEnvelope)
            {
                Envelope = TEnvelopeMath.InitMemory(TEnvelopeMath.GetDimensions(aabb));
                TEnvelopeMath.CopyTo(aabb, Envelope);
            }
        }

        internal void RecalculateAABB(Func<TElement, TEnvelope> getAABB)
        {
            Span<Node> childSpan = CollectionsMarshal.AsSpan(Children);

            TEnvelope firstChild = childSpan[0].GetEnvelope(getAABB);

            InitMemory(firstChild);

            for (int i = 1; i < childSpan.Length; i++)
            {
                MergeEnvelope(Children[i].GetEnvelope(getAABB));
            }
        }

        public override string ToString()
        {
            return $"ParentNode: {Children.Count} children, AABB: {TEnvelopeMath.ToString(Envelope)}";
        }
    }

    internal sealed class LeafNode(TElement element) : Node
    {
        public TElement Element { get; } = element;

        internal override TEnvelope GetEnvelope(Func<TElement, TEnvelope> getAABB)
        {
            return getAABB(Element);
        }

        public override string ToString()
        {
            return $"LeafNode: {Element}";
        }
    }
}

#endif