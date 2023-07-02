// See https://aka.ms/new-console-template for more information

//using Akade.IndexedSet.Benchmarks;
using BenchmarkDotNet.Running;
using System.Reflection;

_ = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
//FullTextIndexBenchmarks test = new();
//GC.Collect();
//Console.WriteLine("Press to start");
//Console.ReadKey();
//Console.WriteLine("started");

//for (int i = 0; i < 1000; i++)
//{
//    _ = test.FuzzyContains_IndexedSet();
//}