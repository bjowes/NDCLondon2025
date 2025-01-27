using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.Diagnostics.Tracing.AutomatedAnalysis;

namespace OneBRC.Benchmark;

[MemoryDiagnoser]
public class RangeVsSlice {
    private static readonly ReadOnlyMemory<byte> utf8Line = "London;1.111"u8.ToArray();

    [Benchmark(Baseline = true)]
    public int Slice() {
        var span = utf8Line.Span;
        var index = span.IndexOf((byte)';');
        return span.Slice(index).Length;
    }

    [Benchmark]
    public int Range() {
        var span = utf8Line.Span;
        var index = span.IndexOf((byte)';');
        return span[index..].Length;
    }
}
