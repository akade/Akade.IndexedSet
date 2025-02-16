using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class RangeIndexTest(IComparer<int> comparer)
        : BaseIndexTest<int, Container<int>, RangeIndex<Container<int>, int>, IComparer<int>>(x => x.Value)
        , IBaseIndexTest<IComparer<int>>
    {
        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => true;

        public static IEnumerable<IComparer<int>> GetComparers()
        {
            return [Comparer<int>.Default];
        }

        protected override RangeIndex<Container<int>, int> CreateIndex()
        {
            return new RangeIndex<Container<int>, int>(comparer, "Test");
        }

        protected override Container<int>[] GetNonUniqueData()
        {
            return
            [
                new(11),
                new(11),
                new(12),
                new(12),
                new(13),
                new(13),
            ];
        }

        protected override Container<int>[] GetUniqueData()
        {
            return
            [
                new(1),
                new(2),
                new(3),
                new(4),
                new(5),
                new(6),
            ];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(GetRangeIndexTestMethods), DynamicDataSourceType.Method)]
    public void RangeIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<RangeIndexTest, IComparer<int>>(method, comparer);
    }

    public static IEnumerable<object[]> GetRangeIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<RangeIndexTest, IComparer<int>>();
    }
}