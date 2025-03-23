using Akade.IndexedSet.Indices;
using Akade.IndexedSet.StringUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class PrefixIndexTest(IEqualityComparer<char> comparer)
        : BaseIndexTest<string, Container<string>, PrefixIndex<Container<string>>, IEqualityComparer<char>>(x => x.Value, comparer)
        , IBaseIndexTest<IEqualityComparer<char>>

    {

        protected override bool SupportsNonUniqueKeys => true;

        protected override bool SupportsRangeBasedQueries => false;
        protected override bool SupportsStartsWithQueries => true;

        public static IEnumerable<IEqualityComparer<char>> GetComparers()
        {
            return [EqualityComparer<char>.Default, CharEqualityComparer.OrdinalIgnoreCase];
        }

        protected override PrefixIndex<Container<string>> CreateIndex()
        {
            return new PrefixIndex<Container<string>>(_comparer, "Test");
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
    [DynamicData(nameof(GetPrefixIndexTestMethods), DynamicDataSourceType.Method)]
    public void PrefixIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<PrefixIndexTest, IEqualityComparer<char>>(method, comparer);
    }

    public static IEnumerable<object[]> GetPrefixIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<PrefixIndexTest, IEqualityComparer<char>>();
    }
}