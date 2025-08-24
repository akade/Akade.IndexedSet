using Akade.IndexedSet.Benchmarks;
using BenchmarkDotNet.Running;
using System.Reflection;

//SpatialIndexBenchmark b = new();
//await b.SetupAsync();

//await Task.Delay(3000);

//for (int i = 0; i < 100000; i++)
//{
//    b.IndexedSet_Knn10_NotBulkLoaded();
//}


_ = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
