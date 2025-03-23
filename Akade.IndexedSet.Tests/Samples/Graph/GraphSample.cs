using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Akade.IndexedSet.Tests.Samples.Graph;

[TestClass]
[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "In unit tests: readability > performance")]
public class GraphSample
{
    [TestMethod]
    public void Efficiently_query_incoming_edges_on_nodes()
    {
        IndexedSet<int, Node> _graph = IndexedSetBuilder<Node>.Create(x => x.Id)
                                                              .WithIndex<int>(x => x.ConnectedTo) // for special collections such as immutable arrays: make sure the correct overload has been selected by providing generic arguments
                                                              .Build();

        _ = _graph.Add(new Node(id: 1, connectedTo: [2, 3, 4]));
        _ = _graph.Add(new Node(id: 2, connectedTo: [1, 2]));
        _ = _graph.Add(new Node(id: 3, connectedTo: [2, 4]));
        _ = _graph.Add(new Node(id: 4, connectedTo: [2, 3, 1]));

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
