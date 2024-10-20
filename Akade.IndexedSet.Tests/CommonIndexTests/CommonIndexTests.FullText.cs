using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class FullTextIndexTest : BaseIndexTest<string, Container<string>, FullTextIndex<Container<string>>>
    {
        public FullTextIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => false;
        protected override bool SupportsStartsWithQueries => true;
        protected override bool SupportsContainsQueries => true;

        protected override FullTextIndex<Container<string>> CreateIndex()
        {
            return new FullTextIndex<Container<string>>(x => x.Value, "Test");
        }

        protected override string GetNotExistingKey()
        {
            return "zzz";
        }

        protected override Container<string>[] GetNonUniqueData()
        {
            return
            [
                new("Mice"),
                new("Possum"),
                new("Possum"),
                new("Rat"),
                new("Rat"),
                new("Rabbit"),
            ];
        }

        protected override Container<string>[] GetUniqueData()
        {
            return
            [
               new("Common snapping turtle"),
               new("Common mallard"),
               new("Domestic dog"),
               new("Goat"),
               new("Mice"),
               new("Rabbit"),
               new("Possum"),
            ];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(GetFullTextIndexTestMethods), DynamicDataSourceType.Method)]
    public void FullTextIndex(string method)
    {
        BaseIndexTest.RunTest<FullTextIndexTest>(method);
    }

    public static IEnumerable<object[]> GetFullTextIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<FullTextIndexTest>();
    }
}
