using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class NonUniqueIndexTest : BaseIndexTest<int, Container<int>, NonUniqueIndex<Container<int>, int>>
    {
        public NonUniqueIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override NonUniqueIndex<Container<int>, int> CreateIndex()
        {
            return new NonUniqueIndex<Container<int>, int>(EqualityComparer<int>.Default, "x => x.Value");
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
    [DynamicData(nameof(GetNonUniqueIndexTestMethods), DynamicDataSourceType.Method)]
    public void NonUniqueIndex(string method)
    {
        BaseIndexTest.RunTest<NonUniqueIndexTest>(method);
    }

    public static IEnumerable<object[]> GetNonUniqueIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<NonUniqueIndexTest>();
    }
}