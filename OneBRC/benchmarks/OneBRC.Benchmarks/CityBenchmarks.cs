
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace OneBRC.Benchmark;

public class CityMemoryPool {
    private readonly byte[] _pool = new byte[16384];
    private int _next;
    public ReadOnlyMemory<byte> GetOrAdd(ReadOnlySpan<byte> key) {
        var span = _pool.AsSpan();
        int index = span.IndexOf(key);
        if (index != -1) {
            return _pool.AsMemory(index, key.Length);
        }
        key.CopyTo(span.Slice(_next));
        _next += key.Length;
        return _pool.AsMemory(index, key.Length);
    }
}

[MemoryDiagnoser]
public class CityBenchmarks {

    StringPool _pool = new StringPool();
    private static readonly CityMemoryPool cityMemoryPool = new CityMemoryPool();
    private static readonly ReadOnlyMemory<byte> ThousandTxt = File.ReadAllBytes("../../../../../../../../../thousand.txt");

    [Benchmark(Baseline = true)]
    public int CountAsStrings() {
        var dict = new Dictionary<string, int>();

        var span = ThousandTxt.Span;
        foreach (var range in span.Split((byte)'\n')) {
            var line = span[range];
            if (line.Length == 0) continue;
            var semicolon = line.IndexOf((byte)';');
            var utf8City = line.Slice(0, semicolon);
            var city = Encoding.UTF8.GetString(utf8City);
            dict[city] = dict.TryGetValue(city, out var count) ? count + 1 : 1;
        }
        return dict.Count;
    }

    [Benchmark]
    public int CountAsPooledStrings() { // Not faster but less allocation
        var dict = new Dictionary<string, int>();

        var span = ThousandTxt.Span;
        foreach (var range in span.Split((byte)'\n')) {
            var line = span[range];
            if (line.Length == 0) continue;
            var semicolon = line.IndexOf((byte)';');
            var utf8City = line.Slice(0, semicolon);
            var city = _pool.GetOrAdd(utf8City, Encoding.UTF8);
            dict[city] = dict.TryGetValue(city, out var count) ? count + 1 : 1;
        }
        return dict.Count;
    }

    [Benchmark]
    public int CountAsCustomPooledStrings() { // TODO not done
        var dict = new Dictionary<string, int>();

        var span = ThousandTxt.Span;
        foreach (var range in span.Split((byte)'\n')) {
            var line = span[range];
            if (line.Length == 0) continue;
            var semicolon = line.IndexOf((byte)';');
            var utf8City = line.Slice(0, semicolon);
            var city = _pool.GetOrAdd(utf8City, Encoding.UTF8);
            dict[city] = dict.TryGetValue(city, out var count) ? count + 1 : 1;
        }
        return dict.Count;
    }

    [Benchmark]
    public int CountAsKeys() {
        var dict = new Dictionary<CityKey, int>();

        var span = ThousandTxt.Span;
        foreach (var range in span.Split((byte)'\n')) {
            var line = span[range];
            if (line.Length == 0) continue;
            var semicolon = line.IndexOf((byte)';');
            var utf8City = line.Slice(0, semicolon);
            var key = CityKey.Create(utf8City);
            dict[key] = dict.TryGetValue(key, out var count) ? count + 1 : 1;
        }
        return dict.Count;
    }
}

public readonly struct CityKey : IEquatable<CityKey> {
    private readonly ulong _simple;
    private readonly Vector512<ulong> _vector;

    public CityKey(ulong simple) {
        _simple = simple;
        _vector = default;
    }

    public CityKey(Vector512<ulong> vector) {
        _simple = 0;
        _vector = vector;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is CityKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        if (_simple != 0) return (int)_simple;
        return _vector.GetHashCode();
    }

    public static CityKey Create(ReadOnlySpan<byte> span) {
        if (span.Length == 8) {
            return new CityKey(MemoryMarshal.Read<ulong>(span));
        }

        if (span.Length < 9) {
            Span<byte> buffer = stackalloc byte[8];
            span.CopyTo(buffer);
            return new CityKey(MemoryMarshal.Read<ulong>(buffer));
        }

        Span<byte> buffer2 = stackalloc byte[64];
        span.CopyTo(buffer2);
        Span<ulong> vector = MemoryMarshal.Cast<byte, ulong>(buffer2);
        return new CityKey(Vector512.Create<ulong>(vector));
    }

    public bool Equals(CityKey other)
    {
         if (_simple != 0) return _simple == other._simple;
        return _vector == other._vector;
    }
}