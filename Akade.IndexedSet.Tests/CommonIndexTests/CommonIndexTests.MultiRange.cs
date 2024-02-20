using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class MultiRangeIndexTest : BaseIndexTest<int, Container<int>, MultiRangeIndex<Container<int>, int>>
    {
        public MultiRangeIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => true;

        protected override MultiRangeIndex<Container<int>, int> CreateIndex()
        {
            return new MultiRangeIndex<Container<int>, int>("Test");
        }

        protected override Container<int>[] GetNonUniqueData()
        {
            return new Container<int>[]
            {
                new(11),
                new(11),
                new(12),
                new(12),
                new(13),
                new(13),
            };
        }

        protected override Container<int>[] GetUniqueData()
        {
            return new Container<int>[]
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
    [DynamicData(nameof(GetMultiRangeIndexTestMethods), DynamicDataSourceType.Method)]
    public void MultiRangeIndex(string method)
    {
        BaseIndexTest.RunTest<MultiRangeIndexTest>(method);
    }

    public static IEnumerable<object[]> GetMultiRangeIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<RangeIndexTest>();
    }
}