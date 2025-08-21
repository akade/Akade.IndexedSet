using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;

internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{

    internal abstract class Node
    {
        internal abstract TEnvelope GetEnvelope(Func<TElement, TEnvelope> getAABB);
    }

    internal sealed class ParentNode : Node
    {
        public List<Node> Children = [];
        private TEnvelope _envelope;

        public ParentNode()
        {

        }

        public ParentNode(Span<Node> span, Func<TElement, TEnvelope> getAABB)
        {
            Children.AddRange(span);
            RecalculateAABB(getAABB);
        }

        public TEnvelope Envelope => _envelope;

        internal override TEnvelope GetEnvelope(Func<TElement, TEnvelope> getAABB)
        {
            return Envelope;
        }

        public bool HasInitializedEnvelope { get; private set; }

        internal void MergeEnvelope(TEnvelope other)
        {
            if (!HasInitializedEnvelope)
            {
                _envelope = other;
                HasInitializedEnvelope = true;
            }
            else
            {
                TEnvelopeMath.Merge(Envelope, other, ref _envelope);
            }
        }

        private void InitMemory(TEnvelope aabb)
        {
            if (!HasInitializedEnvelope)
            {
                _envelope = TEnvelopeMath.Empty(TEnvelopeMath.GetDimensions(aabb));
                TEnvelopeMath.CopyTo(aabb, ref _envelope);
                HasInitializedEnvelope = true;
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

