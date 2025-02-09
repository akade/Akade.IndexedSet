using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

[TestClass]
public partial class CommonIndexTests
{
    internal class UniqueIndexTest : BaseIndexTest<int, Container<int>, UniqueIndex<Container<int>, int>>
    {
        public UniqueIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => false;

        protected override UniqueIndex<Container<int>, int> CreateIndex()
        {
            return new UniqueIndex<Container<int>, int>(EqualityComparer<int>.Default, "x => x.Value");
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
    public void UniqueIndex(string method)
    {
        BaseIndexTest.RunTest<UniqueIndexTest>(method);
    }

    public static IEnumerable<object[]> GetUniqueIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<UniqueIndexTest>();
    }
}