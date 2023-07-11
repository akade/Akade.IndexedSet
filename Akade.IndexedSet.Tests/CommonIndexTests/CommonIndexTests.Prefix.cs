using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class PrefixIndexTest : BaseIndexTest<string, string, Container<string>, PrefixIndex<Container<string>>>
    {
        public PrefixIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => false;
        protected override bool SupportsStartsWithQueries => true;

        protected override PrefixIndex<Container<string>> CreateIndex()
        {
            return new PrefixIndex<Container<string>>(x => x.Value, "Test");
        }

        protected override string GetNotExistingKey()
        {
            return "zzz";
        }

        protected override Container<string>[] GetNonUniqueData()
        {
            return new Container<string>[]
            {
                new("Mice"),
                new("Possum"),
                new("Possum"),
                new("Rat"),
                new("Rat"),
                new("Rabbit"),
            };
        }

        protected override Container<string>[] GetUniqueData()
        {
            return new Container<string>[]
            {
                new("Common snapping turtle"),
                new("Common mallard"),
                new("Domestic dog"),
                new("Goat"),
                new("Mice"),
                new("Rabbit"),
                new("Possum"),
            };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(GetPrefixIndexTestMethods), DynamicDataSourceType.Method)]
    public void PrefixIndex(string method)
    {
        BaseIndexTest.RunTest<PrefixIndexTest>(method);
    }

    public static IEnumerable<object[]> GetPrefixIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<PrefixIndexTest>();
    }
}