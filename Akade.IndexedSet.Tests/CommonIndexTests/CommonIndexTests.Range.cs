using Akade.IndexedSet.Indices;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class RangeIndexTest(IComparer<int> comparer)
        : BaseIndexTest<int, Container<int>, RangeIndex<Container<int>, int>, IComparer<int>>(x => x.Value, comparer)
        , IBaseIndexTest<IComparer<int>>
    {
        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => true;

        public static IEnumerable<IComparer<int>> GetComparers()
        {
            return [Comparer<int>.Default, Comparer<int>.Create((a, b) => (a % 5).CompareTo(b % 5))];
        }

        protected override RangeIndex<Container<int>, int> CreateIndex()
        {
            return new RangeIndex<Container<int>, int>(_comparer, "Test");
        }

        protected override Container<int>[] GetNonUniqueData()
        {
            if (_comparer == Comparer<int>.Default)
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
            else
            {
                return
                [
                    new(11), // 1
                    new(21), // 1
                    new(12), // 2
                    new(17), // 2
                    new(13), // 3
                    new(23), // 3
                ];
            }
        }

        protected override Container<int>[] GetUniqueData()
        {
            if (_comparer == Comparer<int>.Default)
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
            else
            {
                // 1-4 under modulo 5
                return
                [
                    new(1),
                    new(12),
                    new(3),
                    new(24),
                ];
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetRangeIndexTestMethods))]
    public void RangeIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<RangeIndexTest, IComparer<int>>(method, comparer);
    }

    public static IEnumerable<object[]> GetRangeIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<RangeIndexTest, IComparer<int>>();
    }
}