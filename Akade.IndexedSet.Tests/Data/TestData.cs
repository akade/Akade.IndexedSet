namespace Akade.IndexedSet.Tests.Data;

public record TestData(int PrimaryKey, int IntProperty, Guid GuidProperty, string StringProperty);

public record DenormalizedTestData(int PrimaryKey, IEnumerable<int> IntList);