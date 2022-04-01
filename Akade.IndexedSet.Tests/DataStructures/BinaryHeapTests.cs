using Akade.IndexedSet.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.DataStructures;

[TestClass]
public class BinaryHeapTests
{
    private BinaryHeap<int> _heap = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _heap = new();
    }

    [TestMethod]
    public void adding_random_values_including_duplicates_gives_sorted_list_and_correct_insertion_positions()
    {
        Assert.AreEqual(0, _heap.Add(2)); // 2
        Assert.AreEqual(1, _heap.Add(3)); // 2 3
        Assert.AreEqual(0, _heap.Add(1)); // 1 2 3
        Assert.AreEqual(1, _heap.Add(2)); // 1 2 2 3
        Assert.AreEqual(4, _heap.Add(4)); // 1 2 2 3 4
        Assert.AreEqual(4, _heap.Add(4)); // 1 2 2 3 4 4

        CollectionAssert.AreEqual(new[] { 1, 2, 2, 3, 4, 4 }, _heap.ToArray());
    }

    [TestMethod]
    public void removing_random_values_from_heap_preserves_sorted_list_and_reports_correct_removal_positions()
    {
        _heap.AddRange(new[] { 1, 2, 4, 4, 6, 6, 8, 9 });

        Assert.AreEqual(5, _heap.RemoveValue(6)); // 1 2 4 4 6 8 9
        Assert.AreEqual(4, _heap.RemoveValue(6)); // 1 2 4 4 8 9
        Assert.AreEqual(0, _heap.RemoveValue(1)); // 2 4 4 8 9
        Assert.AreEqual(3, _heap.RemoveValue(8)); // 2 4 4 9
        CollectionAssert.AreEqual(new[] { 2, 4, 4, 9 }, _heap.ToArray());
    }

    [TestMethod]
    public void querying_by_single_values_returns_correct_ranges()
    {
        _heap.AddRange(new[] { 1, 2, 4, 4, 4, 4, 6, 6, 8, 9 });

        Assert.AreEqual(0..1, _heap.GetRange(1));
        Assert.AreEqual(1..2, _heap.GetRange(2));
        Assert.AreEqual(2..6, _heap.GetRange(4));
        Assert.AreEqual(6..8, _heap.GetRange(6));
        Assert.AreEqual(8..9, _heap.GetRange(8));
        Assert.AreEqual(9..10, _heap.GetRange(9));
    }



    [TestMethod]
    public void querying_by_single_values_returns_correct_position_with_zero_length_when_empty()
    {
        Assert.AreEqual(0..0, _heap.GetRange(2));
    }

    [TestMethod]
    public void querying_by_single_values_returns_correct_position_with_zero_length_when_no_matching_value_is_found()
    {
        _ = _heap.Add(5);
        _ = _heap.Add(8);
        Assert.AreEqual(1..1, _heap.GetRange(6));
    }

    [TestMethod]
    public void querying_by_range_returns_empty_range_when_empty()
    {
        Assert.AreEqual(0..0, _heap.GetRange(3, 7, inclusiveStart: true, inclusiveEnd: true));
    }

    [TestMethod]
    public void querying_by_range_values_returns_correct_position_with_zero_length_when_no_matching_value_is_found()
    {
        _ = _heap.Add(5);
        _ = _heap.Add(8);
        Assert.AreEqual(1..1, _heap.GetRange(6, 7));
    }

    [TestMethod]
    public void querying_by_range_returns_correct_ranges()
    {
        //                     0  1  2  3  4  5  6  7  8  9
        _heap.AddRange(new[] { 1, 2, 4, 4, 4, 4, 6, 6, 8, 9 });
        // case 1:             0  1
        // case 2:             0  -  -  -  -  -  -  -  -  9
        // case 3:                1  -  -  -  5
        // case 4:                   2  -  -  -  -  7

        Assert.AreEqual(0..2, _heap.GetRange(0, 3));
        Assert.AreEqual(0..10, _heap.GetRange(0, 10));

        Assert.AreEqual(1..6, _heap.GetRange(2, 6));
        Assert.AreEqual(2..8, _heap.GetRange(4, 8));
    }

    [TestMethod]
    public void querying_by_range_returns_correct_ranges_respecting_inclusive_or_exclusive_boundaries_when_boundaries_are_elements()
    {
        //                     0  1  2  3  4  5  6  7  8  9  10
        _heap.AddRange(new[] { 1, 2, 4, 4, 4, 4, 5, 6, 6, 8, 9 });
        // case 1:                               6  -  8
        // case 1:                   2  -  -  -  -  -  8
        // case 1:                               6  -  -  9
        // case 1:                   2  -  -  -  -  -  -  9

        Assert.AreEqual(6..9, _heap.GetRange(4, 8, inclusiveStart: false, inclusiveEnd: false));
        Assert.AreEqual(2..9, _heap.GetRange(4, 8, inclusiveStart: true, inclusiveEnd: false));
        Assert.AreEqual(6..10, _heap.GetRange(4, 8, inclusiveStart: false, inclusiveEnd: true));
        Assert.AreEqual(2..10, _heap.GetRange(4, 8, inclusiveStart: true, inclusiveEnd: true));
    }

    [TestMethod]
    public void querying_by_range_returns_correct_ranges_respecting_inclusive_or_exclusive_boundaries_when_boundaries_are_not_elements()
    {
        //                     0  1  2  3  4  5  6  7  8  9  10
        _heap.AddRange(new[] { 1, 2, 4, 4, 4, 4, 5, 6, 6, 8, 9 });
        // all cases:                2  -  -  -  -  -  8

        Assert.AreEqual(2..9, _heap.GetRange(3, 7, inclusiveStart: false, inclusiveEnd: false));
        Assert.AreEqual(2..9, _heap.GetRange(3, 7, inclusiveStart: true, inclusiveEnd: false));
        Assert.AreEqual(2..9, _heap.GetRange(3, 7, inclusiveStart: false, inclusiveEnd: true));
        Assert.AreEqual(2..9, _heap.GetRange(3, 7, inclusiveStart: true, inclusiveEnd: true));
    }
}
