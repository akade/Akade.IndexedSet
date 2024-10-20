using System.Collections.Immutable;

namespace Akade.IndexedSet.Tests.Samples.Graph;

public class Node(int id, ImmutableArray<int> connectedTo)
{
    public int Id { get; } = id;
    public ImmutableArray<int> ConnectedTo { get; } = connectedTo;
}
