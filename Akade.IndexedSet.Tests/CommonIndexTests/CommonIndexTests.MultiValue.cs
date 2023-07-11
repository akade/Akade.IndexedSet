using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class MultiValueIndexTest : BaseIndexTest<int[], int, Container<int[]>, MultiValueIndex<Container<int[]>, int>>
    {
        public MultiValueIndexTest() : base(x => x.Value)
        {

        }

        protected override bool SupportsNonUniqueKeys => true;

        protected override MultiValueIndex<Container<int[]>, int> CreateIndex()
        {
            return new MultiValueIndex<Container<int[]>, int>(x => x.Value, "x => x.Primitive");
        }

        protected override Container<int[]>[] GetNonUniqueData()
        {
            return new Container<int[]>[]
            {
                new(new[] { 1, 2 }),
                new(new[] { 2, 3 }),
                new(new[] { 2, 3 }),
                new(new[] { 3, 4 }),
            };
        }

        protected override Container<int[]>[] GetUniqueData()
        {
            return new Container<int[]>[]
            {
                new(new[] { 1, 2 }),
                new(new[] { 3, 4 }),
                new(new[] { 5, 6 }),
                new(new[] { 7, 8 }),
            };
        }

        protected override int SearchKeyFromIndexKey(int[] key)
        {
            return key[0];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(GetMultiValueIndexTestMethods), DynamicDataSourceType.Method)]
    public void MultiValueIndex(string method)
    {
        BaseIndexTest.RunTest<MultiValueIndexTest>(method);
    }

    public static IEnumerable<object[]> GetMultiValueIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<MultiValueIndexTest>();
    }
}