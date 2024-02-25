using Bogus;
using Bogus.DataSets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Samples.Search;

#pragma warning disable AkadeIndexedSetEXP0001 // The Api for FullTextIndex with multiple keys is marked as experimental but the feature is production ready.
[TestClass]
public class NGramsSample
{
    private readonly Rant _rantGenerator;
    private readonly IndexedSet<string> _set;

    public NGramsSample()
    {
        _rantGenerator = new Rant()
        {
            Random = new Randomizer(42)
        };

        _set = _rantGenerator.Reviews(lines: 1000)
                             .ToIndexedSet()
                             .WithFullTextIndex(x => NGrams(x, 3))
                             .Build();
    }

    private static IEnumerable<string> NGrams(string content, int ngramSize)
    {
        content = content.ToLowerInvariant();
        for (int i = 0; i < content.Length - ngramSize + 1; i++)
        {
            yield return content.Substring(i, ngramSize);
        }
    }

    [TestMethod]
    public void Search_for_person()
    {
        // Combine search results for each n-gram
        const string searchTerm = "hobbies";
        string[] ngrams = NGrams(searchTerm, 3).ToArray();
        string[] searchResult = ngrams.SelectMany(rant => _set.Contains(x => NGrams(x, 3), rant))
                                      .Where(rant => rant.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)) // Filter out false positives
                                      .Distinct()
                                      .ToArray();


        Assert.IsTrue(searchResult.Length > 0);
    }
}

#pragma warning restore AkadeIndexedSetEXP0001

