using Akade.IndexedSet.Indices;
using Akade.IndexedSet.StringUtilities;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class FullTextIndexTest(IEqualityComparer<char> comparer)
        : BaseIndexTest<string, Container<string>, FullTextIndex<Container<string>>, IEqualityComparer<char>>(x => x.Value, comparer)
        , IBaseIndexTest<IEqualityComparer<char>>
    {
        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => false;
        protected override bool SupportsStartsWithQueries => true;
        protected override bool SupportsContainsQueries => true;

        protected override FullTextIndex<Container<string>> CreateIndex()
        {
            return new FullTextIndex<Container<string>>(x => x.Value, _comparer, "Test");
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

        public static IEnumerable<IEqualityComparer<char>> GetComparers()
        {
            return [EqualityComparer<char>.Default, CharEqualityComparer.OrdinalIgnoreCase];
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetFullTextIndexTestMethods))]
    public void FullTextIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<FullTextIndexTest, IEqualityComparer<char>>(method, comparer);
    }

    public static IEnumerable<object[]> GetFullTextIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<FullTextIndexTest, IEqualityComparer<char>>();
    }
}
