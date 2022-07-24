// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using System.Reflection;
_ = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);