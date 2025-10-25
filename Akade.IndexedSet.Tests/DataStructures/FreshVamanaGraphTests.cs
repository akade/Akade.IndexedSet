#if NET9_0_OR_GREATER
using Akade.IndexedSet.DataStructures.FreshVamana;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.Tests.DataStructures;

[TestClass]
public class FreshVamanaGraphTests
{
    private class TestData(float[] data)
    {
        public float[] Data { get; } = data;
    }

    private readonly Random _random = new(42);
    private readonly List<TestData> _randomTestData;

    public FreshVamanaGraphTests()
    {
        _randomTestData = Enumerable.Range(0, 1000)
            .Select(_ =>
            {
                float[] values = Enumerable.Range(0, 128)
                    .Select(__ => _random.NextSingle())
                    .ToArray();

                return new TestData(values);
            })
            .ToList();
    }

    [TestMethod]
    public void NearestNeighbors_returns_closest_items()
    {
        FreshVamanaGraph<TestData> graph = new(x => x.Data.AsSpan());

        foreach (TestData item in _randomTestData)
        {
            graph.Add(item);
        }

        // Calculate top 5 recall for every item
        _ = AssertRecall(graph);
    }

    private float AssertRecall(FreshVamanaGraph<TestData> graph)
    {
        int found = 0;

        foreach (TestData item in _randomTestData)
        {
            IEnumerable<TestData> neighbors = graph.NeareastNeighbors(item.Data.AsSpan(), 5);

            if (neighbors.Contains(item))
            {
                found++;
            }
        }

        float recall = (float)found / _randomTestData.Count;
        Assert.IsGreaterThanOrEqualTo(0.9f, recall, $"Recall was too low: {recall:P2}");
        return recall;
    }

    [TestMethod]
    public void Stability_when_deleting_items()
    {
        FreshVamanaGraph<TestData> graph = new(x => x.Data.AsSpan());

        foreach (TestData item in _randomTestData)
        {
            graph.Add(item);
        }
        Console.WriteLine($"Built from scratch: recall: {AssertRecall(graph):P2}");

        List<TestData> items = new(50);

        for (int i = 0; i < 5; i++)
        {
            items.Clear();
            // Select 50 distinct random items to delete and re-add
            while (items.Count < 50)
            {
                TestData candidate = _randomTestData[_random.Next(0, _randomTestData.Count)];
                if (!items.Contains(candidate))
                {
                    items.Add(candidate);
                }
            }

            foreach (TestData? item in items)
            {
                graph.Delete(item);
            }

            Console.WriteLine();

            foreach (TestData? item in items)
            {
                graph.Add(item);
            }
            Console.WriteLine($"Iteration {i + 1} complete: recall: {AssertRecall(graph):P2}");
        }
    }

}

#endif
