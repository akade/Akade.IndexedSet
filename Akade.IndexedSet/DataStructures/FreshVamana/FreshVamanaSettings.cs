#if NET9_0_OR_GREATER

namespace Akade.IndexedSet.DataStructures.FreshVamana;

/// <summary>
/// Settings for FreshVamana graph
/// </summary>
/// <param name="FlatThreshold"> Parameter that controls when to switch from a flat search over all elements to the approximate search on the Vamana graph. 
/// Note that the graph is being constructed even if below the threshold.</param>
/// <param name="SearchListSize"> Parameter L that controls the search list size during insertion and search, i.e. on how many nodes are the greedy search is performed</param>
/// <param name="Alpha">Paramater that regularizes short vs long connections within the Vamana Graph</param>
/// <param name="OutDegreeBound">Parameter R, i.e. the maximum of outgoing conenctions from each node</param>
/// <param name="DeletionThreshold">The threshold </param>
internal readonly record struct FreshVamanaSettings(int FlatThreshold, int SearchListSize, float Alpha, int OutDegreeBound, float DeletionThreshold)
{
    public static readonly FreshVamanaSettings Default = new(FlatThreshold: 200, SearchListSize: 125, Alpha: 1.2f, OutDegreeBound: 70, DeletionThreshold: 0.5f);
}

#endif