namespace Akade.IndexedSet.Tests.Data;

public record TestData(int PrimaryKey, int IntProperty, Guid GuidProperty, string StringProperty)
{
    public int WritableProperty { get; set; }
}

public record DenormalizedTestData(int PrimaryKey, IEnumerable<int> IntList);