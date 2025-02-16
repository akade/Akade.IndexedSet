using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;
internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex, TComparer>
{

    [BaseTestMethod]
    public void Single_should_return_matching_element()
    {
        TElement[] data = GetUniqueData();
        TIndex index = CreateIndexWithData(data);
        Assert.AreEqual(data[0], index.Single(_keyAccessor(data[0])));
    }

    [BaseTestMethod]
    public void Single_should_throw_if_empty()
    {
        TIndex index = CreateIndex();
        _ = Assert.ThrowsException<KeyNotFoundException>(() => index.Single(GetNotExistingKey()));
    }

    [BaseTestMethod]
    public void Single_should_throw_if_not_found()
    {
        TIndex index = CreateIndexWithData(GetUniqueData());
        _ = Assert.ThrowsException<KeyNotFoundException>(() => index.Single(GetNotExistingKey()));
    }

    [BaseTestMethod]
    public void Single_should_throw_if_multiple_entries_are_found()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);
            TIndexKey nonUniqueKey = data.GroupBy(_keyAccessor).Where(x => x.Count() > 1).First().Key;
            _ = Assert.ThrowsException<InvalidOperationException>(() => index.Single(nonUniqueKey));
        }
    }

    [BaseTestMethod]
    public void TryGetSingle_should_return_false_if_empty()
    {
        TIndex index = CreateIndex();
        Assert.IsFalse(index.TryGetSingle(GetNotExistingKey(), out _));
    }

    [BaseTestMethod]
    public void TryGetSingle_should_return_false_if_key_is_not_present()
    {
        TIndex index = CreateIndexWithData(GetUniqueData());
        Assert.IsFalse(index.TryGetSingle(GetNotExistingKey(), out _));
    }

    [BaseTestMethod]
    public void TryGetSingle_should_return_true_for_matching_element()
    {
        TElement[] data = GetUniqueData();
        TIndex index = CreateIndexWithData(data);
        Assert.IsTrue(index.TryGetSingle(_keyAccessor(data[0]), out TElement? element));
        Assert.AreEqual(data[0], element);
    }

    [BaseTestMethod]
    public void TryGetSingle_should_return_false_if_multiple_entries_are_found()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);
            TIndexKey nonUniqueKey = data.GroupBy(_keyAccessor).Where(x => x.Count() > 1).First().Key;
            Assert.IsFalse(index.TryGetSingle(nonUniqueKey, out _));
        }
    }

}