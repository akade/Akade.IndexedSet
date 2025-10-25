
namespace Akade.IndexedSet.DataStructures.FreshVamana;
#if NET9_0_OR_GREATER

internal partial class FreshVamanaGraph<TElement> where TElement : notnull
{
    private class SearchList(int maxSize)
    {
        private readonly List<float> _distances = new(capacity: maxSize + 1);
        private readonly List<FreshVamanaNode> _nodes = new(capacity: maxSize + 1);
        private readonly int _maxSize = maxSize;
        private readonly HashSet<FreshVamanaNode> _visited = new(capacity: maxSize * 2);

        private int _firstUnvisitedCandidate;

        public HashSet<FreshVamanaNode> Visited => _visited;

        public bool Add(FreshVamanaNode node, float distance)
        {
            if (_nodes.Count + 1 >= _maxSize
                && distance > _distances[^1])
            {
                return false;
            }

            int index = _distances.BinarySearch(distance);
            if (index < 0)
            {
                index = ~index;
            }
            else if (_nodes[index] == node)
            {
                return false;
            }
            _distances.Insert(index, distance);
            _nodes.Insert(index, node);

            if (_nodes.Count > _maxSize)
            {
                _distances.RemoveAt(_distances.Count - 1);
                _nodes.RemoveAt(_nodes.Count - 1);
            }

            _firstUnvisitedCandidate = Math.Min(_firstUnvisitedCandidate, index);

            return true;
        }

        public bool AddVisisted(FreshVamanaNode node)
        {
            return _visited.Add(node);
        }

        public FreshVamanaNode? GetClosestNotVisited()
        {
            while (_firstUnvisitedCandidate < _nodes.Count)
            {
                FreshVamanaNode candidate = _nodes[_firstUnvisitedCandidate];
                if (!_visited.Contains(candidate))
                {
                    return candidate;
                }
                _firstUnvisitedCandidate++;
            }
            return null;
        }

        internal IEnumerable<TElement> GetClosestK(int k)
        {
            return _nodes.Take(k).Select(n => n.Element);
        }
    }
}

#endif
