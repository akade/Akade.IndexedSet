using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class MultiRangeIndexTest(IComparer<int> comparer)
        : BaseIndexTest<int, Container<int>, MultiRangeIndex<Container<int>, int>, IComparer<int>>(x => x.Value, comparer)
        , IBaseIndexTest<IComparer<int>>
    {
        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => true;

        public static IEnumerable<IComparer<int>> GetComparers()
        {
            return [Comparer<int>.Default];
        }

        protected override MultiRangeIndex<Container<int>, int> CreateIndex()
        {
            return new MultiRangeIndex<Container<int>, int>(_comparer, "Test");
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
    [DynamicData(nameof(GetMultiRangeIndexTestMethods), DynamicDataSourceType.Method)]
    public void MultiRangeIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<MultiRangeIndexTest, IComparer<int>>(method, comparer);
    }

    public static IEnumerable<object[]> GetMultiRangeIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<MultiRangeIndexTest, IComparer<int>>();
    }
}