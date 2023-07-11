using System.Diagnostics;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
internal class Container<T>
{
    public Container(T value)
    {

        Value = value;
    }

    public T Value { get; }
}
