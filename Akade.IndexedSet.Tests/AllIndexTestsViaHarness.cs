using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

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
    internal class UniqueIndexViaHarness : BaseIndexTest<int, SimpleTestData, UniqueIndex<SimpleTestData, int>>
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
    internal class NonUniqueIndexViaHarness : BaseIndexTest<int, SimpleTestData, NonUniqueIndex<SimpleTestData, int>>
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
    internal class MultiValueIndexViaHarness : BaseIndexTest<int, MultiValueTestData, MultiValueIndex<MultiValueTestData, int>>
    {
        public MultiValueIndexViaHarness() : base(x => x.Primitive)
        {

        }

        protected override bool SupportsMultiValueKeys => true;

        protected override bool SupportsNonUniqueKeys => throw new NotImplementedException();

        protected override MultiValueIndex<MultiValueTestData, int> CreateIndex()
        {
            return new MultiValueIndex<MultiValueTestData, int>(x => x.Primitive, "x => x.Primitive");
        }

        protected override MultiValueTestData[] GetMultiValueData()
        {
            return new MultiValueTestData[]
            {
                new(11),
                new(11),
                new(12),
                new(12),
                new(13),
                new(13),
            };
        }

        protected override MultiValueTestData[] GetNonUniqueData()
        {
            throw new NotImplementedException();
        }

        protected override MultiValueTestData[] GetUniqueData()
        {
            return new MultiValueTestData[]
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
        IndexHelper.RunTest<MultiValueIndexViaHarness>(method);
    }

    public static IEnumerable<object[]> GetTestMethods()
    {
        return IndexHelper.GetTestMethods<MultiValueIndexViaHarness>();
    }
}
