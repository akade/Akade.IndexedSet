using Bogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Samples.Search;

#pragma warning disable AkadeIndexedSetEXP0001 // The Api for FullTextIndex with multiple keys is marked as experimental but the feature is production ready.
[TestClass]
public class SearchSamples
{
    private readonly IndexedSet<Person> _set = Enumerable.Range(0, 1000)
                                                         .Select(i => new Person())
                                                         .ToIndexedSet()
                                                         .WithFullTextIndex(SearchIndex)
                                                         .Build();

    private static IEnumerable<string> SearchIndex(Person x)
    {
        // you can add all the fields you want to be searchable here
        // Preprocessing such as `ToLowerInvariant` can also be done here
        yield return x.FirstName.ToLowerInvariant();
        yield return x.LastName.ToLowerInvariant();
        yield return x.Email.ToLowerInvariant();
    }

    private static string[] Tokenize(string input)
    {
        // Apply the same preprocessing steps as in SearchIndex
        return input.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static int TokenCountMatch(Person x, string[] tokens)
    {
        return tokens.Count(token => SearchIndex(x).Any(key => key.Contains(token, StringComparison.InvariantCultureIgnoreCase)));
    }

    [TestMethod]
    public void Search_for_person()
    {
        // Combine search results for each token
        string[] tokens = Tokenize("John Doe").ToArray();
        Person[] searchResult = tokens.SelectMany(x => _set.Contains(SearchIndex, x))
                                      .Distinct()
                                      .ToArray();

        // in practice, order the results by TokenCountMatch or verify that all tokens are present.
        Assert.IsTrue(searchResult.All(x => TokenCountMatch(x, tokens) > 0));
    }
}

#pragma warning restore AkadeIndexedSetEXP0001

