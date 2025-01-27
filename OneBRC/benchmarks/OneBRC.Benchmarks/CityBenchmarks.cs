
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OneBRC.Benchmark;

[MemoryDiagnoser]
public class CityBenchmarks {
    private static readonly ReadOnlyMemory<byte> ThousandTxt = File.ReadAllBytes("../../../../../../../../../thousand.txt");

    [Benchmark]
    public int Test() {
        return ThousandTxt.Length;
    }
}
