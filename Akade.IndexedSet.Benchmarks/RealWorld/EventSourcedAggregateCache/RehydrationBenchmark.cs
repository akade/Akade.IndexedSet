using Akade.IndexedSet.Concurrency;
using BenchmarkDotNet.Attributes;

namespace Akade.IndexedSet.Benchmarks.RealWorld.EventSourcedAggregateCache;

[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
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
