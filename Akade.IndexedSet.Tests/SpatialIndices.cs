
using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.TestUtilities;
using System.Numerics;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class SpatialIndices
{
    private IndexedSet<Vector2> _indexedSet = null!;


    [TestInitialize]
    public void Init()
    {
        Random r = Random.Shared;
        IEnumerable<Vector2> points = Enumerable.Range(0, 1000)
                                                .Select(_ => new Vector2((float)r.NextDouble() * 1000, (float)r.NextDouble() * 1000))
                                                .ToList();

        _indexedSet = points.ToIndexedSet()
                            .WithSpatialIndex(x => x)
                            .Build();
    }

    [TestMethod]
    public void AreaQuery()
    {
        Vector2 min = new(200, 200);
        Vector2 max = new(300, 300);
        
        Vector2[] expectedResults = _indexedSet.FullScan()
                                               .Where(p => p.X >= min.X && p.Y >= min.Y && p.X <= max.X && p.Y <= max.Y)
                                               .OrderBy(p => p.X)
                                               .ThenBy(p => p.Y)
                                               .ToArray();

#pragma warning disable AkadeIndexedSetEXP0002 
        Vector2[] results = _indexedSet.Intersects(x => x, min, max)
                                       .OrderBy(p => p.X)
                                       .ThenBy(p => p.Y)
                                       .ToArray();
#pragma warning restore AkadeIndexedSetEXP0002 

        CollectionAssert.AreEqual(expectedResults, results);
    }

    [TestMethod]
    public void SingleQuery()
    {
        Vector2 point = _indexedSet.FullScan().First();
        Vector2 result = _indexedSet.Single(x => x, point);
        Assert.AreEqual(point, result);
    }

    [TestMethod]
    public void KNNQuery()
    {
        Vector2 point = new(500, 500);
        int k = 10;
        Vector2[] expectedResults = _indexedSet.FullScan()
                                               .OrderBy(p => Vector2.DistanceSquared(p, point))
                                               .Take(k)
                                               .ToArray();
        Vector2[] results = _indexedSet.NearestNeighbors(x => x, point).Take(10).ToArray();
        CollectionAssert.AreEqual(expectedResults, results);
    }
}
