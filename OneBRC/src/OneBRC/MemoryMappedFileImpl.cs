using System.IO.MemoryMappedFiles;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using OneBRC.Shared;

namespace OneBRC;


public class MemoryMappedFileImpl {
    private const int BufferSize = 1024 * 1024;
    private readonly string _filePath;

    public MemoryMappedFileImpl(string filePath)
    {
        _filePath = filePath;
    }

// unsafe must be specifically allowed in the project
    public unsafe ValueTask Run()
    {
        var size = new FileInfo(_filePath).Length;
        var threadCount = Environment.ProcessorCount * 8;

        using var mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var view = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);
        
        var chunks = MemoryMappedFileAnalyzer.Analyze(mmf, size, threadCount);

        Parallel.ForEach(chunks, new ParallelOptions {
            MaxDegreeOfParallelism = Environment.ProcessorCount
         }, chunk => chunk.Run(view));

        var dictionary = chunks[0].Dictionary;
        for (int i = 1; i < threadCount; i++) {
            foreach (var (cityKey, value) in chunks[i].Dictionary) {
                ref var acc = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, cityKey, out bool existed);
                if (!existed) acc.SetCity(value.City);
                acc.Combine(value);
            }
        }

        foreach (var accumulator in dictionary.Values.OrderBy(a => a.City))
        {
            Console.WriteLine($"{accumulator.City}: {accumulator.Min:F1}/{accumulator.Mean:F1}/{accumulator.Max:F1}");
        }

        return default;        
    }

/*
    private void ProcessLine(Span<byte> line, Dictionary<ulong, IntAccumulator> dictionary)
    {
        var delimit = line.IndexOf((byte)';');
        var cityUtf8 = line.Slice(0, delimit);
        var valueUtf8 = line.Slice(delimit + 1);
        var cityHash = FastHash.HashBytes(cityUtf8);
        ref var acc = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, cityHash, out bool existed);
        if (!existed) {
            acc.SetCity(Encoding.UTF8.GetString(cityUtf8));
        }

        var value = FastParse.FloatAsInt(valueUtf8);
        acc.Record(value);
    }
    */
}

public static class MemoryMappedFileAnalyzer {
    public static unsafe MemoryMappedFileChunk[] Analyze(MemoryMappedFile mmf, long size, int threadCount) {
        var chunks = new MemoryMappedFileChunk[threadCount];

        using var accessor = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);
        byte* pointer = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
        var esitmatedChunkSize = size / threadCount;
        long offset = 0;
        for (int i = 0; i < threadCount - 1; i++) {
            var span = new ReadOnlySpan<byte>(pointer + offset, (int)esitmatedChunkSize);
            var lastNewline = span.LastIndexOf((byte)'\n');
            var actualSize = lastNewline + 1;
            chunks[i] = new MemoryMappedFileChunk(offset, actualSize, i);
            offset += actualSize;
        }

        /* [^1] range operator for last item */
        var last = chunks[^1] = new MemoryMappedFileChunk(offset, (int)(size - offset), threadCount - 1);
        return chunks;
    }
}

public class MemoryMappedFileChunk
{
    private long offset;
    private int actualSize;
    private int i;
    private readonly Dictionary<ulong, IntAccumulator> _dictionary = new();

    public Dictionary<ulong, IntAccumulator> Dictionary => _dictionary;

    public MemoryMappedFileChunk(long offset, int actualSize, int i)
    {
        this.offset = offset;
        this.actualSize = actualSize;
        this.i = i;
    }

    public unsafe void Run(MemoryMappedViewAccessor view) {
        byte* pointer = null;
        view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
        var span = new ReadOnlySpan<byte>(pointer + offset, actualSize);

        while (span.Length > 0) {
            var eol = span.IndexOf((byte)'\n');
            if (eol == -1) break;
            var line = span.Slice(0, eol);
            ProcessLine(line);
            span = span.Slice(eol + 1);
        }
    } 

    private void ProcessLine(ReadOnlySpan<byte> line)
    {
        var delimit = line.IndexOf((byte)';');
        var cityUtf8 = line.Slice(0, delimit);
        var valueUtf8 = line.Slice(delimit + 1);
        var cityHash = FastHash.HashBytes(cityUtf8);
        ref var acc = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, cityHash, out bool existed);
        if (!existed) {
            acc.SetCity(Encoding.UTF8.GetString(cityUtf8));
        }

        var value = FastParse.FloatAsInt(valueUtf8);
        acc.Record(value);
    }

}