namespace Akade.IndexedSet.Indices;

/// <summary>
/// Non-generic on the index key to have a strongly typed base class for an index
/// </summary>
internal abstract class Index<TElement>
{
    public string Name { get; }

    protected Index(string name)
    {
        Name = name;
    }

    public abstract void Clear();
}