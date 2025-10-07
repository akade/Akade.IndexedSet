using Akade.IndexedSet.Indices;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class NonUniqueIndexTest(IEqualityComparer<int> comparer)
        : BaseIndexTest<int, Container<int>, NonUniqueIndex<Container<int>, int>, IEqualityComparer<int>>(x => x.Value, comparer)
        , IBaseIndexTest<IEqualityComparer<int>>
    {
        protected override bool SupportsNonUniqueKeys => true;

        public static IEnumerable<IEqualityComparer<int>> GetComparers()
        {
            return [EqualityComparer<int>.Default];
        }

        protected override NonUniqueIndex<Container<int>, int> CreateIndex()
        {
            return new NonUniqueIndex<Container<int>, int>(_comparer, "x => x.Value");
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

    [TestMethod]
    [DynamicData(nameof(GetNonUniqueIndexTestMethods))]
    public void NonUniqueIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<NonUniqueIndexTest, IEqualityComparer<int>>(method, comparer);
    }

    public static IEnumerable<object[]> GetNonUniqueIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<NonUniqueIndexTest, IEqualityComparer<int>>();
    }
}