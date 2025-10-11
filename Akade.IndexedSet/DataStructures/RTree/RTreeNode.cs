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

        internal void MergeEnvelope(ref TEnvelope other)
        {
            if (!HasInitializedEnvelope)
            {
                Envelope = other;
                HasInitializedEnvelope = true;
            }
            else
            {
                TEnvelopeMath.Merge(ref Envelope, ref other, ref Envelope);
            }
        }

        private void InitMemory(ref TEnvelope aabb)
        {
            if (!HasInitializedEnvelope)
            {
                Envelope = TEnvelopeMath.Empty(TEnvelopeMath.GetDimensions(ref aabb));
                TEnvelopeMath.CopyTo(ref aabb, ref Envelope);
                HasInitializedEnvelope = true;
            }
        }

        internal void RecalculateAABB()
        {
            Span<Node> childSpan = CollectionsMarshal.AsSpan(Children);

            ref TEnvelope firstChild = ref childSpan[0].Envelope;

            InitMemory(ref firstChild);

            for (int i = 1; i < childSpan.Length; i++)
            {
                MergeEnvelope(ref Children[i].Envelope);
            }
        }

        public override string ToString()
        {
            return $"ParentNode: {Children.Count} children, AABB: {TEnvelopeMath.ToString(ref Envelope)}";
        }
    }

    internal sealed class LeafNode(TElement element, TEnvelope envelope) : Node(envelope)
    {
        public TElement Element { get; } = element;

        public override string ToString()
        {
            return $"LeafNode: {Element}";
        }
    }
}

