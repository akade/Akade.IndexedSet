using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Akade.IndexedSet.Tests;

[DebuggerDisplay($"{{{nameof(Primitive)}}}")]
internal class SimpleTestData
{
    public SimpleTestData(int primitive)
    {
        Primitive = primitive;
    }

    public int Primitive { get; }
}

[TestClass]
public class HarnessUniqueIndex
{
    internal class UniqueIndexViaHarness : BaseIndexTest<int, int, SimpleTestData, UniqueIndex<SimpleTestData, int>>
    {
        public UniqueIndexViaHarness() : base(x => x.Primitive)
        {

        }

        protected override bool SupportsNonUniqueKeys => false;

        protected override UniqueIndex<SimpleTestData, int> CreateIndex()
        {
            return new UniqueIndex<SimpleTestData, int>(x => x.Primitive, "x => x.Primitive");
        }

        protected override SimpleTestData[] GetNonUniqueData()
        {
            return new SimpleTestData[]
            {
                new(11),
                new(11),
                new(12),
                new(12),
                new(13),
                new(13),
            };
        }

        protected override SimpleTestData[] GetUniqueData()
        {
            return new SimpleTestData[]
            {
                new(1),
                new(2),
                new(3),
                new(4),
                new(5),
                new(6),
            };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(IndexHelper.GetTestMethods), DynamicDataSourceType.Method)]
    public void Test(string method)
    {
        IndexHelper.RunTest<UniqueIndexViaHarness>(method);
    }

    public static IEnumerable<object[]> GetTestMethods()
    {
        return IndexHelper.GetTestMethods<UniqueIndexViaHarness>();
    }

}

[TestClass]
public class HarnessNonUniqueIndex
{
    internal class NonUniqueIndexViaHarness : BaseIndexTest<int, int, SimpleTestData, NonUniqueIndex<SimpleTestData, int>>
    {
        public NonUniqueIndexViaHarness() : base(x => x.Primitive)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override NonUniqueIndex<SimpleTestData, int> CreateIndex()
        {
            return new NonUniqueIndex<SimpleTestData, int>(x => x.Primitive, "x => x.Primitive");
        }

        protected override SimpleTestData[] GetNonUniqueData()
        {
            return new SimpleTestData[]
            {
                new(11),
                new(11),
                new(12),
                new(12),
                new(13),
                new(13),
            };
        }

        protected override SimpleTestData[] GetUniqueData()
        {
            return new SimpleTestData[]
            {
                new(1),
                new(2),
                new(3),
                new(4),
                new(5),
                new(6),
            };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(IndexHelper.GetTestMethods), DynamicDataSourceType.Method)]
    public void Test(string method)
    {
        IndexHelper.RunTest<NonUniqueIndexViaHarness>(method);
    }

    public static IEnumerable<object[]> GetTestMethods()
    {
        return IndexHelper.GetTestMethods<NonUniqueIndexViaHarness>();
    }
}

[DebuggerDisplay($"{{{nameof(Primitive)}}}")]
internal class MultiValueTestData
{
    public MultiValueTestData(params int[] primitive)
    {
        Primitive = primitive;
    }

    public int[] Primitive { get; }
}

[TestClass]
public class HarnessMultiValueIndex
{
    internal class MultiValueIndexViaHarness : BaseIndexTest<int[], int, MultiValueTestData, MultiValueIndex<MultiValueTestData, int>>
    {
        public MultiValueIndexViaHarness() : base(x => x.Primitive)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override MultiValueIndex<MultiValueTestData, int> CreateIndex()
        {
            return new MultiValueIndex<MultiValueTestData, int>(x => x.Primitive, "x => x.Primitive");
        }

        protected override MultiValueTestData[] GetNonUniqueData()
        {
            return new MultiValueTestData[]
            {
                new(1, 2),
                new(2, 3),
                new(2, 3),
                new(3, 4),
            };
        }

        protected override MultiValueTestData[] GetUniqueData()
        {
            return new MultiValueTestData[]
            {
                new(1, 2),
                new(3, 4),
                new(5, 6),
                new(7, 8),
            };
        }

        protected override int SearchKeyFromIndexKey(int[] key)
        {
            return key[0];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(IndexHelper.GetTestMethods), DynamicDataSourceType.Method)]
    public void Test(string method)
    {
        IndexHelper.RunTest<MultiValueIndexViaHarness>(method);
    }

    public static IEnumerable<object[]> GetTestMethods()
    {
        return IndexHelper.GetTestMethods<MultiValueIndexViaHarness>();
    }
}

[TestClass]
public class HarnessRangeIndex
{
    internal class HarnessRangeIndexViaHarness : BaseIndexTest<int, int, SimpleTestData, RangeIndex<SimpleTestData, int>>
    {
        public HarnessRangeIndexViaHarness() : base(x => x.Primitive)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => true;

        protected override RangeIndex<SimpleTestData, int> CreateIndex()
        {
            return new RangeIndex<SimpleTestData, int>(x => x.Primitive, "Test");
        }

        protected override SimpleTestData[] GetNonUniqueData()
        {
            return new SimpleTestData[]
            {
                new(11),
                new(11),
                new(12),
                new(12),
                new(13),
                new(13),
            };
        }

        protected override SimpleTestData[] GetUniqueData()
        {
            return new SimpleTestData[]
            {
                new(1),
                new(2),
                new(3),
                new(4),
                new(5),
                new(6),
            };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(IndexHelper.GetTestMethods), DynamicDataSourceType.Method)]
    public void Test(string method)
    {
        IndexHelper.RunTest<HarnessRangeIndexViaHarness>(method);
    }

    public static IEnumerable<object[]> GetTestMethods()
    {
        return IndexHelper.GetTestMethods<HarnessRangeIndexViaHarness>();
    }
}

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public class SimpleStringData
{
    public string Value { get; }

    public SimpleStringData(string value)
    {
        Value = value;
    }
}

[TestClass]
public class HarnessPrefixIndex
{
    internal class HarnessPrefixIndexViaHarness : BaseIndexTest<string, string, SimpleStringData, PrefixIndex<SimpleStringData>>
    {
        public HarnessPrefixIndexViaHarness() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => false;
        protected override bool SupportsStartsWithQueries => true;

        protected override PrefixIndex<SimpleStringData> CreateIndex()
        {
            return new PrefixIndex<SimpleStringData>(x => x.Value, "Test");
        }

        protected override string GetNotExistingKey()
        {
            return "zzz";
        }

        protected override SimpleStringData[] GetNonUniqueData()
        {
            return new SimpleStringData[]
            {
                new("Mice"),
                new("Possum"),
                new("Possum"),
                new("Rat"),
                new("Rat"),
                new("Rabbit"),
            };
        }

        protected override SimpleStringData[] GetUniqueData()
        {
            return new SimpleStringData[]
            {
               new("Mice"),
               new("Possum"),
               new("Rat"),
               new("An"),
               new("Dog"),
               new("Goat"),
               new("Pig"),
               new("Rabbit"),
            };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(IndexHelper.GetTestMethods), DynamicDataSourceType.Method)]
    public void Test(string method)
    {
        IndexHelper.RunTest<HarnessPrefixIndexViaHarness>(method);
    }

    public static IEnumerable<object[]> GetTestMethods()
    {
        return IndexHelper.GetTestMethods<HarnessPrefixIndexViaHarness>();
    }
}