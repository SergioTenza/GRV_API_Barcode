using BenchmarkDotNet.Running;
using Benchmarks;

BenchmarkRunner.Run<BenchmarkHarness>();
Console.ReadKey();