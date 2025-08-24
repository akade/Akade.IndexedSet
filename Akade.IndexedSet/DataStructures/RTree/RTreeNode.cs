using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;

internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{

    internal abstract class Node(TEnvelope envelope)
    {
        public TEnvelope Envelope = envelope;
    }

    internal sealed class ParentNode : Node
    {
        public List<Node> Children = [];

        public ParentNode() : base(default)
        {

        }

        public ParentNode(Span<Node> span) : base(default)
        {
            Children.AddRange(span);
            RecalculateAABB();
        }

        public bool HasInitializedEnvelope { get; private set; }

        internal void MergeEnvelope(TEnvelope other)
        {
            if (!HasInitializedEnvelope)
            {
                Envelope = other;
                HasInitializedEnvelope = true;
            }
            else
            {
                TEnvelopeMath.Merge(Envelope, other, ref Envelope);
            }
        }

        private void InitMemory(TEnvelope aabb)
        {
            if (!HasInitializedEnvelope)
            {
                Envelope = TEnvelopeMath.Empty(TEnvelopeMath.GetDimensions(aabb));
                TEnvelopeMath.CopyTo(aabb, ref Envelope);
                HasInitializedEnvelope = true;
            }
        }

        internal void RecalculateAABB()
        {
            Span<Node> childSpan = CollectionsMarshal.AsSpan(Children);

            TEnvelope firstChild = childSpan[0].Envelope;

            InitMemory(firstChild);

            for (int i = 1; i < childSpan.Length; i++)
            {
                MergeEnvelope(Children[i].Envelope);
            }
        }

        public override string ToString()
        {
            return $"ParentNode: {Children.Count} children, AABB: {TEnvelopeMath.ToString(Envelope)}";
        }
    }

    internal sealed class LeafNode : Node
    {
        public TElement Element { get; }

        public LeafNode(TElement element, TEnvelope envelope) : base(envelope)
        {
            Element = element;
        }

        public override string ToString()
        {
            return $"LeafNode: {Element}";
        }
    }
}

