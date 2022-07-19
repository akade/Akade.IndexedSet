// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;

_ = BenchmarkRunner.Run(typeof(Program).Assembly);