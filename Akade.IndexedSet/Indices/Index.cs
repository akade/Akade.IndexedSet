namespace Akade.IndexedSet.Indices;

/// <summary>
/// Non-generic on the index key to have a strongly typed base class for an index
/// </summary>
internal abstract class Index<TPrimaryKey, TElement>
    where TPrimaryKey : notnull
{
    public string Name { get; }

    protected Index(string name)
    {
        Name = name;
    }

    public abstract void Add(TElement value);
    public abstract void Remove(TElement value);
    public virtual void AddRange(IEnumerable<TElement> elementsToAdd)
    {
        foreach (TElement element in elementsToAdd)
            Add(element);
    }
}