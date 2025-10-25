namespace Akade.IndexedSet.DataStructures.FreshVamana;
#if NET9_0_OR_GREATER

internal partial class FreshVamanaGraph<TElement> where TElement : notnull
{
    private class FreshVamanaNode(TElement element)
    {
        public TElement Element { get; } = element;
        public HashSet<FreshVamanaNode> Neighbors { get; } = [];

#if DEBUG

        private static int _nextId;

        public int Id { get; } = Interlocked.Increment(ref _nextId);

        public override string ToString()
        {
            return $"Node {Id} ({Element})";
        }
#endif
    }
}

#endif
