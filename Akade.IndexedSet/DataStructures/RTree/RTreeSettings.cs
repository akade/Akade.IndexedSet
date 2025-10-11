namespace Akade.IndexedSet.DataStructures.RTree;
internal readonly record struct RTreeSettings(int MinNodeEntries, int MaxNodeEntries, int ReinsertionCount)
{
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxNodeEntries, 4);
        ArgumentOutOfRangeException.ThrowIfLessThan(MinNodeEntries, 1);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(MinNodeEntries, (MaxNodeEntries + 1) / 2);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(ReinsertionCount, MaxNodeEntries - MinNodeEntries);

    }

    public static RTreeSettings Default => new(3, 6, 2);
}
