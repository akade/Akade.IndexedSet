using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

[TestClass]
public partial class CommonIndexTests
{
    internal class UniqueIndexTest(IEqualityComparer<int> comparer)
        : BaseIndexTest<int, Container<int>, UniqueIndex<Container<int>, int>, IEqualityComparer<int>>(x => x.Value)
        , IBaseIndexTest<IEqualityComparer<int>>
    {
        protected override bool SupportsNonUniqueKeys => false;

        public static IEnumerable<IEqualityComparer<int>> GetComparers()
        {
            return [EqualityComparer<int>.Default];
        }

        protected override UniqueIndex<Container<int>, int> CreateIndex()
        {
            return new UniqueIndex<Container<int>, int>(comparer, "x => x.Value");
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
    [DynamicData(nameof(GetUniqueIndexTestMethods), DynamicDataSourceType.Method)]
    public void UniqueIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<UniqueIndexTest, IEqualityComparer<int>>(method, comparer);
    }

    public static IEnumerable<object[]> GetUniqueIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<UniqueIndexTest, IEqualityComparer<int>>();
    }
}