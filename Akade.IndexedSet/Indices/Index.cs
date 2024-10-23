namespace Akade.IndexedSet.Indices;

/// <summary>
/// Non-generic on the index key to have a strongly typed base class for an index
/// </summary>
internal abstract class Index<TElement>(string name)
{
    public string Name { get; } = name;

    public abstract void Clear();
}