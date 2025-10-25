using System.Diagnostics;
using System.Numerics.Tensors;

namespace Akade.IndexedSet.DataStructures.FreshVamana;
#if NET9_0_OR_GREATER

internal partial class FreshVamanaGraph<TElement>(Func<TElement, ReadOnlySpan<float>> accessor)
    where TElement : notnull
{
    private readonly HashSet<FreshVamanaNode> _nodes = new();
    private readonly Dictionary<TElement, FreshVamanaNode> _elementToNode = new();
    private readonly HashSet<FreshVamanaNode> _deletedNodes = new();

    private SearchList GreedySearch(FreshVamanaNode entryPoint, ReadOnlySpan<float> query, int k, int searchListSize)
    {
        Debug.Assert(k > 0);
        Debug.Assert(searchListSize >= k);

        if (k > _nodes.Count)
        {
            k = _nodes.Count;
        }

        SearchList searchList = new(searchListSize);

        float entryDistance = Distance(accessor(entryPoint.Element), query);
        searchList.Add(entryPoint, entryDistance);

        while (searchList.GetClosestNotVisited() is FreshVamanaNode closest)
        {
            _ = searchList.AddVisisted(closest);

            foreach (FreshVamanaNode neighbor in closest.Neighbors)
            {
                float neighborDistance = Distance(accessor(neighbor.Element), query);
                _ = searchList.Add(neighbor, neighborDistance);
            }
        }

        return searchList;
    }

    private void Insert(FreshVamanaNode entryPoint, TElement element, int searchListSize, float alpha, int outdegreeBound)
    {
        FreshVamanaNode newNode = new(element);
        _nodes.Add(newNode);
        _elementToNode.Add(element, newNode);

        ReadOnlySpan<float> value = accessor(element);

        SearchList searchList = GreedySearch(entryPoint, value, 1, searchListSize);

        RobustPrune(newNode, searchList.Visited, alpha, outdegreeBound);

        foreach (FreshVamanaNode j in newNode.Neighbors)
        {
            _ = j.Neighbors.Add(newNode);

            if (j.Neighbors.Count > outdegreeBound)
            {
                RobustPrune(j, j.Neighbors, alpha, outdegreeBound);
            }
        }
    }


    private void RobustPrune(FreshVamanaNode node, HashSet<FreshVamanaNode> candidates, float alpha, int outdegreeBound)
    {
        PriorityQueue<FreshVamanaNode, float> queue = new();

        foreach (FreshVamanaNode candidate in candidates.Union(node.Neighbors))
        {
            if (candidate != node)
            {
                float distance = Distance(accessor(candidate.Element), accessor(node.Element));
                queue.Enqueue(candidate, distance);
            }
        }


        node.Neighbors.Clear();
        ReadOnlySpan<float> nodePosition = accessor(node.Element);

        HashSet<FreshVamanaNode> removedFromQueue = new(); // C#'s priority queue does not support (fast) removal, so we track removed nodes separately

        while (queue.TryDequeue(out FreshVamanaNode? closest, out _))
        {
            if (removedFromQueue.Contains(closest)) continue;

            _ = node.Neighbors.Add(closest);

            if (node.Neighbors.Count >= outdegreeBound)
            {
                break;
            }

            ReadOnlySpan<float> closestPos = accessor(closest.Element);

            foreach ((FreshVamanaNode other, _) in queue.UnorderedItems)
            {
                if (removedFromQueue.Contains(closest)) continue;

                ReadOnlySpan<float> otherPos = accessor(other.Element);

                float distanceToClosest = Distance(closestPos, otherPos); // 𝑑 (𝑝∗, 𝑝′)
                float distanceToNode = Distance(nodePosition, otherPos); // 𝑑 (𝑝, 𝑝′)

                if (alpha * distanceToClosest < distanceToNode)
                {
                    _ = removedFromQueue.Add(other);
                }
            }
        }
    }

    public bool Delete(TElement element)
    {
        if (!_elementToNode.TryGetValue(element, out FreshVamanaNode? node))
        {
            return false;
        }

        _deletedNodes.Add(node);

        if (_deletedNodes.Count >= 0.05 * _nodes.Count)
        {
            Delete(_deletedNodes);
            _deletedNodes.Clear();
        }

        return true;
    }

    private void Delete(HashSet<FreshVamanaNode> toDelete)
    {
        foreach (FreshVamanaNode node in _nodes)
        {
            if (toDelete.Contains(node))
            {
                continue;
            }

            int neighborCount = node.Neighbors.Count;

            node.Neighbors.ExceptWith(toDelete);

            if (node.Neighbors.Count < neighborCount)
            {
                foreach (FreshVamanaNode neighbor in node.Neighbors.ToArray()) // TODO: optimize
                {
                    node.Neighbors.UnionWith(neighbor.Neighbors);
                }
                node.Neighbors.ExceptWith(toDelete);

                RobustPrune(node, node.Neighbors, alpha: 1.2f, outdegreeBound: 20);
            }
        }

        foreach (FreshVamanaNode node in toDelete)
        {
            _ = _nodes.Remove(node);
            _ = _elementToNode.Remove(node.Element);
        }
    }

    public bool Add(TElement element)
    {
        if (_elementToNode.TryGetValue(element, out FreshVamanaNode? existingNode))
        {
            if (_deletedNodes.Remove(existingNode))
            {
                return true;
            }

            return false;
        }

        if (_nodes.Count == 0)
        {
            _nodes.Add(new FreshVamanaNode(element));
            _elementToNode.Add(element, _nodes.First());
        }
        else
        {
            Insert(_nodes.First(), element, searchListSize: 50, alpha: 1.2f, outdegreeBound: 20);
        }

        return true;
    }

    internal IEnumerable<TElement> NeareastNeighbors(Span<float> query, int k)
    {
        int searchListSize = _deletedNodes.Count + 50;
        return GreedySearch(_nodes.First(), query, k, searchListSize).GetClosestK(k);
    }


    private float Distance(ReadOnlySpan<float> x, ReadOnlySpan<float> y)
    {
        return 1 - TensorPrimitives.CosineSimilarity(x, y);
    }
}

#endif
