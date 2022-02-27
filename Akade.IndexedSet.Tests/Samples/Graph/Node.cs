using System.Collections.Immutable;

namespace Akade.IndexedSet.Tests.Samples.Graph;

public class Node
{
    public Node(int id, ImmutableArray<int> connectedTo)
    {
        Id = id;
        ConnectedTo = connectedTo;
    }

    public int Id { get; }
    public ImmutableArray<int> ConnectedTo { get; }
}
