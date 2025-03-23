using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Samples.Leaderboard;

[TestClass]
public class LeaderboardSample
{
    private readonly IndexedSet<int, LeaderboardEntry> _leaderboard = IndexedSetBuilder<LeaderboardEntry>.Create(x => x.Id)
        .WithRangeIndex(x => x.Score)
        .WithRangeIndex(x => x.Timestamp)
        .Build();
    private readonly DateTimeOffset _now;

    public LeaderboardSample()
    {
        _now = DateTimeOffset.UtcNow;

        for (int i = 0; i <= 240; i++)
        {
            int daysInPast = i / 24;
            int hour = i % 24;

            _ = _leaderboard.Add(new LeaderboardEntry(i, i * i, _now.AddDays(-daysInPast).AddHours(hour)));
        }
    }


    [TestMethod]
    public void Get_overall_highscore()
    {
        Assert.AreEqual(240 * 240, _leaderboard.Max(x => x.Score));
    }

    [TestMethod]
    public void Get_top_ten()
    {
        var expected = Enumerable.Range(231, 10) // 231 to 240 (inclusive)
                                 .Select(i => i * i)
                                 .Take(10)
                                 .Reverse()
                                 .ToList();

        var actual = _leaderboard.OrderByDescending(x => x.Score)
                                 .Take(10)
                                 .Select(x => x.Score)
                                 .ToList();

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Get_second_page_of_leaderboard()
    {
        var expected = Enumerable.Range(221, 10) // 221 to 230 (inclusive)
                                 .Select(i => i * i)
                                 .Take(10)
                                 .Reverse()
                                 .ToList();

        var actual = _leaderboard.OrderByDescending(x => x.Score, skip: 10)
                                 .Take(10)
                                 .Select(x => x.Score)
                                 .ToList();

        CollectionAssert.AreEqual(expected, actual);
    }
}
