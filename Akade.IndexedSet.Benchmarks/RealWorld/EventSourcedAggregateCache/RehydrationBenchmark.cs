using Akade.IndexedSet.Benchmarks.RealWorld.EventSourcedAggregateCache;
using Akade.IndexedSet.Concurrency;
using BenchmarkDotNet.Attributes;
using System.Collections.Immutable;

namespace Akade.IndexedSet.Benchmarks.RealWorld;

[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
public class RehydrationBenchmark
{
    private readonly ConcurrentIndexedSet<AggregateId, Aggregate> _set = IndexedSetBuilder<Aggregate>.Create(x => x.Id)
                                                                                                     .WithIndex(x => x.Owner)
                                                                                                     .WithIndex(x => x.SharedWith.IsEmpty)
                                                                                                     .WithIndex(AggregateIndices.TenantsWithAccess)
                                                                                                     .WithFullTextIndex(AggregateIndices.FullName)
                                                                                                     .BuildConcurrent();

    [Benchmark]
    public void Rehydration()
    {
        _set.Clear();
        EventHandlers.HandleEvents(_set, DataGenerator.GenerateEvents(30000));
    }
    
}
