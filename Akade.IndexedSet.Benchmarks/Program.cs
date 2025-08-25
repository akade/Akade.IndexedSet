using BenchmarkDotNet.Running;
using System.Reflection;

_ = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
