
using BenchmarkDotNet.Running;
using OneBRC.Benchmark;

//BenchmarkRunner.Run<FloatParsingBenchmarks>();
//BenchmarkRunner.Run<MultiFloatParsingBenchmark>();
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);