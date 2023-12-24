using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;
internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex>
{
    [TestMethod]
    public void Where_should_return_empty_result_when_no_data_is_present()
    {
        TIndex index = CreateIndex();
        Assert.IsFalse(index.Where(GetNotExistingKey()).Any());
    }

    [TestMethod]
    public void Where_should_return_empty_result_when_key_is_not_present()
    {
        TIndex index = CreateIndexWithData(GetUniqueData());
        Assert.IsFalse(index.Where(GetNotExistingKey()).Any());
    }

    [TestMethod]
    public void Where_should_return_single_element_for_unique_key()
    {
        TElement[] data = GetUniqueData();
        TIndex index = CreateIndexWithData(data);
        Assert.AreEqual(data[0], index.Where(_keyAccessor(data[0])).Single());
    }

    [TestMethod]
    public void Where_should_return_multiple_element_for_unique_key()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            IGrouping<TIndexKey, TElement>[] groups = data.GroupBy(_keyAccessor).Where(g => g.Count() > 1).ToArray();

            TIndex index = CreateIndexWithData(data);

            foreach (IGrouping<TIndexKey, TElement> group in groups)
            {
                CollectionAssert.AreEquivalent(group.ToArray(), index.Where(group.Key).ToArray());
            }
        }
    }
}
