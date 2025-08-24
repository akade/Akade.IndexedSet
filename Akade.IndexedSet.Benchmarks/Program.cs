using Akade.IndexedSet.Benchmarks;
using BenchmarkDotNet.Running;
using System.Reflection;

#if DEBUG
SpatialIndexBenchmark b = new();
await b.SetupAsync();

await Task.Delay(3000);

for (int i = 0; i < 1000000; i++)
{
    b.IndexedSet_Area_NotBulkLoaded();
}

#else

_ = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
#endif