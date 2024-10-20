using System.Diagnostics;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
internal class Container<T>(T value)
{
    public T Value { get; } = value;
}
