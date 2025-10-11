using Akade.IndexedSet.DataStructures.RTree;
using System.Numerics;
using Vec2SpatialIndex = Akade.IndexedSet.Indices.SpatialIndex<Akade.IndexedSet.Tests.CommonIndexTests.Container<System.Numerics.Vector2>, System.Numerics.Vector2, Akade.IndexedSet.VecRec2, float, Akade.IndexedSet.DataStructures.RTree.Vector2Math>;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

public partial class CommonIndexTests
{
    internal class SpatialIndexTest(IComparer<Vector2> comparer)
        : BaseIndexTest<Vector2, Container<Vector2>, Vec2SpatialIndex, IComparer<Vector2>>(x => x.Value, comparer)
        , IBaseIndexTest<IComparer<Vector2>>
    {
        protected override bool SupportsNonUniqueKeys => true;
        protected override bool SupportsSpatialQueries => true;

        protected override float NearestNeighborsDistance(Vector2 a, Vector2 b)
        {
            return Vector2.DistanceSquared(a, b);
        }

        protected override bool Intersects(Vector2 a, Vector2 min, Vector2 max, bool inclusiveBoundary)
        {
            return inclusiveBoundary switch
            {
                true => a.X >= min.X && a.X <= max.X && a.Y >= min.Y && a.Y <= max.Y,
                false => a.X > min.X && a.X < max.X && a.Y > min.Y && a.Y < max.Y,
            };
        }

        public static IEnumerable<IComparer<Vector2>> GetComparers()
        {
            return [Comparer<Vector2>.Create((a,b) => {
                int cmp = a.X.CompareTo(b.X);
                return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
            })];
        }

        protected override Vec2SpatialIndex CreateIndex()
        {
            return new Vec2SpatialIndex(c => VecRec2.CreateFromPoint(c.Value), 2, RTreeSettings.Default, "SpatialIndex");
        }

        protected override Container<Vector2>[] GetNonUniqueData()
        {
            return
            [
                new(new(1, 2)),
                new(new(1, 2)),
                new(new(2, 2)),
                new(new(3, 2)),
                new(new(3, 3)),
                new(new(3, 3)),
                new(new(4, 4)),
            ];
        }

        protected override Container<Vector2>[] GetUniqueData()
        {
            return
            [
                  new(new(1, 2)),
                  new(new(2, 3)),
                  new(new(2, 2)),
                  new(new(3, 3)),
                  new(new(4, 4)),
            ];
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetSpatialIndexTestMethods))]
    public void SpatialIndex(string method, object comparer)
    {
        BaseIndexTest.RunTest<SpatialIndexTest, IComparer<Vector2>>(method, comparer);
    }

    public static IEnumerable<object[]> GetSpatialIndexTestMethods()
    {
        return BaseIndexTest.GetTestMethods<SpatialIndexTest, IComparer<Vector2>>();
    }
}