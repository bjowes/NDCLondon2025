
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OneBRC.Benchmark;

[MemoryDiagnoser]
public class FloatParsingBenchmarks {
    private const byte Semicolon = (byte)';';
    private const byte Newline = (byte)'\n';
    private static readonly string line = "London;1.111";
    private static readonly ReadOnlyMemory<byte> utf8Line = "London;1.111"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> utf8Lines = "London;1.111\nParis;0.222\nNew York;666\n"u8.ToArray();


    
    private static readonly Dictionary<string, float> FloatFinder = Enumerable.Range(0, 200_000)
        .Select(r => (float)(r - 100_000)/1000)
        .ToDictionary(r => r.ToString(), r => r);
/*
    [Benchmark(Baseline = true)]
    public float Naive() {
        return float.Parse(line.Split(';')[1]);
    }

    [Benchmark]
    public float FewerAllocations() {
        var split = line.IndexOf(';');
        var value = line.Substring(split + 1);
        return float.Parse(value);
    }
*/
    [Benchmark]
    public float ZeroAllocationsSpan() {
        var split = line.IndexOf(';');
        var value = line.AsSpan().Slice(split + 1);
        return float.Parse(value);
    }

    [Benchmark]
    public float Utf8ZeroAllocationsSpan() {
        var span = utf8Line.Span;
        var split = span.IndexOf(Semicolon);
        var value = span.Slice(split + 1);
        return float.Parse(value);
    }

    [Benchmark]
    public float FloatFinderLookup() {
        var span = line.AsSpan();
        var split = span.IndexOf(';');
        var value = span.Slice(split + 1);
        return FloatFinder[value.ToString()];
    }
}