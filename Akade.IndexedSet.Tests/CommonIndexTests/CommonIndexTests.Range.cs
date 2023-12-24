using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class RangeIndexTest : BaseIndexTest<int, Container<int>, RangeIndex<Container<int>, int>>
    {
        public RangeIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => true;

        protected override RangeIndex<Container<int>, int> CreateIndex()
        {
            return new RangeIndex<Container<int>, int>("Test");
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
    [DynamicData(nameof(GetRangeIndexTestMethods), DynamicDataSourceType.Method)]
    public void RangeIndex(string method)
    {
        BaseIndexTest.RunTest<RangeIndexTest>(method);
    }

    public static IEnumerable<object[]> GetRangeIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<RangeIndexTest>();
    }
}