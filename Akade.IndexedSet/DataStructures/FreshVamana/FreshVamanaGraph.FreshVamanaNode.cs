#if NET9_0_OR_GREATER
namespace Akade.IndexedSet.DataStructures.FreshVamana;

internal partial class FreshVamanaGraph<TElement>
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
