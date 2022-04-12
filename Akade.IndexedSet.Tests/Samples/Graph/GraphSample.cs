using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;

namespace Akade.IndexedSet.Tests.Samples.Graph;

[TestClass]
public class GraphSample
{
    [TestMethod]
    public void efficiently_query_incoming_edges_on_nodes()
    {
        IndexedSet<int, Node> _graph = IndexedSetBuilder<Node>.Create(x => x.Id)
                                                              .WithIndex<int>(x => x.ConnectedTo) // for special collections such as immutable arrays: make sure the correct overload has been selected by providing generic arguments
                                                              .Build();

        _ = _graph.Add(new Node(id: 1, connectedTo: ImmutableArray.Create(2, 3, 4)));
        _ = _graph.Add(new Node(id: 2, connectedTo: ImmutableArray.Create(1, 2)));
        _ = _graph.Add(new Node(id: 3, connectedTo: ImmutableArray.Create(2, 4)));
        _ = _graph.Add(new Node(id: 4, connectedTo: ImmutableArray.Create(2, 3, 1)));

        // fast variant via index
        var fastResult = _graph.Where(x => x.ConnectedTo, contains: 4)
                               .Select(x => x.Id)
                               .ToList();

        // full enumeration via contains
        var slowResult = _graph.FullScan()
                               .Where(x => x.ConnectedTo.Contains(4))
                               .Select(x => x.Id)
                               .ToList();

        CollectionAssert.AreEquivalent(new[] { 1, 3 }, fastResult);
        CollectionAssert.AreEquivalent(new[] { 1, 3 }, slowResult);
    }
}
