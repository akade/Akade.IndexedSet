using Akade.IndexedSet.Concurrency;
using Akade.IndexedSet.StringUtilities;

namespace Akade.IndexedSet.Tests.Samples;

[TestClass]
public class Readme
{
    private record Data(int PrimaryKey, int SecondaryKey, string Text = "")
    {
        public int MutableProperty { get; set; }

        public IEnumerable<int> AlternativeKeys { get; set; } = [];
    }

    [TestMethod]
    public void Features_UniqueIndex()
    {
        IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
                                                           .WithUniqueIndex(x => x.SecondaryKey)
                                                           .Build();

        _ = set.Add(new(PrimaryKey: 1, SecondaryKey: 5));

        // fast access via primary key
        Data data = set[1];

        // fast access via secondary key
        data = set.Single(x => x.SecondaryKey, 5);
    }

    [TestMethod]
    public void Features_NonUniqueIndex_SingleKey()
    {
        IndexedSet<int, Data>? set = new Data[] { new(PrimaryKey: 1, SecondaryKey: 5), new(PrimaryKey: 2, SecondaryKey: 5) }
            .ToIndexedSet(x => x.PrimaryKey)
            .WithIndex(x => x.SecondaryKey)
            .Build();

        // fast access via secondary key
        IEnumerable<Data> data = set.Where(x => x.SecondaryKey, 5);
    }

    [TestMethod]
    public void Features_UniqueIndex_MultipleKeys()
    {
        IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
                                                   .WithUniqueIndex(x => x.AlternativeKeys) // Where AlternativeKeys returns an IEnumerable<int>
                                                   .Build();

        _ = set.Add(new(PrimaryKey: 1, SecondaryKey: 2) { AlternativeKeys = [3, 4] });
        _ = set.Single(x => x.AlternativeKeys, contains: 3); // returns above element
    }
    private record GraphNode(int Id, IEnumerable<int> ConnectsTo);

    [TestMethod]
    public void Features_NonUniqueIndex_MultipleKeys()
    {
        IndexedSet<int, GraphNode> set = IndexedSetBuilder<GraphNode>.Create(a => a.Id)
                                                                     .WithIndex(x => x.ConnectsTo) // Where ConnectsTo returns an IEnumerable<int>
                                                                     .Build();

        //   1   2
        //   |\ /
        //   | 3
        //    \|
        //     4

        _ = set.Add(new(Id: 1, ConnectsTo: [3, 4]));
        _ = set.Add(new(Id: 2, ConnectsTo: [3]));
        _ = set.Add(new(Id: 3, ConnectsTo: [1, 2, 3]));
        _ = set.Add(new(Id: 4, ConnectsTo: [1, 3]));

        // For readability, it is recommended to write the name for the parameter contains
        IEnumerable<GraphNode> nodesThatConnectTo1 = set.Where(x => x.ConnectsTo, contains: 1); // returns nodes 3 & 4
        IEnumerable<GraphNode> nodesThatConnectTo3 = set.Where(x => x.ConnectsTo, contains: 1); // returns nodes 1 & 2 & 3

        // Non-optimized Where(x => x.Contains(...)) query:
        nodesThatConnectTo1 = set.FullScan().Where(x => x.ConnectsTo.Contains(1)); // returns nodes 3 & 4, but enumerates through the entire set
    }

    [TestMethod]
    public void Features_RangeIndex()
    {
        IndexedSet<Data> set = IndexedSetBuilder.Create(new Data[] { new(1, SecondaryKey: 3), new(2, SecondaryKey: 4) })
                                                .WithRangeIndex(x => x.SecondaryKey)
                                                .Build();

        // fast access via range query
        IEnumerable<Data> data = set.Range(x => x.SecondaryKey, 1, 5);

        // fast max & min key value or elements
        int maxKey = set.Max(x => x.SecondaryKey);
        data = set.MaxBy(x => x.SecondaryKey);

        // fast larger or smaller than
        data = set.LessThan(x => x.SecondaryKey, 4);

        // fast ordering & paging
        data = set.OrderBy(x => x.SecondaryKey, skip: 10).Take(10); // second page of 10 elements
    }

    public record RangeData(int Start, int End);

    [TestMethod]
    public void Features_ComputedOrCompoundKey()
    {
        var data = new RangeData[] { new(Start: 2, End: 10) };
        IndexedSet<RangeData> set = data.ToIndexedSet()
                                        .WithIndex(x => (x.Start, x.End))
                                        .WithIndex(x => x.End - x.Start)
                                        .WithIndex(ComputedKey.SomeStaticMethod)
                                        .Build();
        // fast access via indices
        IEnumerable<RangeData> result = set.Where(x => (x.Start, x.End), (2, 10));
        result = set.Where(x => x.End - x.Start, 8);
        result = set.Where(ComputedKey.SomeStaticMethod, 42);
    }

    [TestMethod]
    public void Features_Concurrency()
    {
        ConcurrentIndexedSet<Data> set = IndexedSetBuilder<Data>.Create()
                                                                .WithIndex(x => x.SecondaryKey)
                                                                .BuildConcurrent();
    }

    [TestMethod]
    public void Features_StringQueries()
    {
        IndexedSet<Type> data = typeof(object).Assembly.GetTypes()
                                                       .ToIndexedSet()
                                                       .WithPrefixIndex(x => x.Name)
                                                       .WithFullTextIndex(x => x.FullName!)
                                                       .Build();

        // fast prefix or contains queries via indices
        _ = data.StartsWith(x => x.Name, "Int");
        _ = data.Contains(x => x.FullName!, "Int");

        // fuzzy searching is supported by prefix and full text indices
        // the following will also match "String"
        _ = data.FuzzyStartsWith(x => x.Name, "Strang", 1);
        _ = data.FuzzyContains(x => x.FullName!, "Strang", 1);
    }

    [TestMethod]
    public void FAQ_MultipleIndicesForSameProperty()
    {
        IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey)
                                                           .WithUniqueIndex(DataIndices.UniqueIndex)
                                                           .WithRangeIndex(x => x.SecondaryKey)
                                                           .Build();
        _ = set.Add(new(1, 4));
        // querying unique index:
        Data data = set.Single(DataIndices.UniqueIndex, 4); // Uses the unique index
        Data data2 = set.Single(x => x.SecondaryKey, 4); // Uses the range index
        IEnumerable<Data> inRange = set.Range(x => x.SecondaryKey, 1, 10); // Uses the range index
    }

    [TestMethod]
    public void FAQ_CaseInsensitiveFuzzyMatching()
    {
        IndexedSet<Data> set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey)
                                                      .WithFullTextIndex(x => x.Text, CharEqualityComparer.OrdinalIgnoreCase)
                                                      .Build();
        IEnumerable<Data> matches = set.FuzzyContains(x => x.Text, "Search", maxDistance: 2);
    }

    [TestMethod]
    public void FAQ_UpdatingKeys()
    {
        IndexedSet<Data> set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey).Build();
        ConcurrentIndexedSet<Data> concurrentSet = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey).BuildConcurrent();
        Data dataElement = new(1, 4);

        // updating a mutable property
        _ = set.Update(dataElement, e => e.MutableProperty = 7);
        // updating an immutable property
        _ = set.Update(dataElement, e => e with { SecondaryKey = 12 });
        // be careful, the second time will do an add as dataElement still refers to the "old" record
        _ = set.Update(dataElement, e => e with { SecondaryKey = 12 });

        // updating in an concurrent set
        concurrentSet.Update(set =>
        {
            // serialized access to the inner IndexedSet, where you can safely use above update methods
            // in an multi-threaded environment
        });
    }

    private record Purchase(int Id, int ProductId, int Amount, decimal UnitPrice);

    [TestMethod]
    public void Overview()
    {
        var data = new Purchase[] {
            new(Id: 1, ProductId: 1, Amount: 1, UnitPrice: 5),
            new(Id: 2, ProductId: 1, Amount: 2, UnitPrice: 5),
            new(Id: 6, ProductId: 4, Amount: 3, UnitPrice: 12),
            new(Id: 7, ProductId: 4, Amount: 8, UnitPrice: 10) // discounted price
            };

        IndexedSet<int, Purchase> set = data.ToIndexedSet(x => x.Id)
                                            .WithIndex(x => x.ProductId)
                                            .WithRangeIndex(x => x.Amount)
                                            .WithRangeIndex(x => x.UnitPrice)
                                            .WithRangeIndex(x => x.Amount * x.UnitPrice)
                                            .WithIndex(x => (x.ProductId, x.UnitPrice))
                                            .Build();

        // efficient queries on configured indices
        _ = set.Where(x => x.ProductId, 4);
        _ = set.Range(x => x.Amount, 1, 3, inclusiveStart: true, inclusiveEnd: true);
        _ = set.GreaterThanOrEqual(x => x.UnitPrice, 10);
        _ = set.MaxBy(x => x.Amount * x.UnitPrice);
        _ = set.Where(x => (x.ProductId, x.UnitPrice), (4, 10));
    }

    private static class DataIndices
    {
        public static int UniqueIndex(Data x)
        {
            return x.SecondaryKey;
        }
    }

    private static class ComputedKey
    {
        public static int SomeStaticMethod(RangeData data)
        {
            return data.Start * data.End;
        }
    }
}
