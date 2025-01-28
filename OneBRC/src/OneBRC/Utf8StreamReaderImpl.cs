using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using OneBRC.Shared;

namespace OneBRC;


public class Utf8StreamReaderImpl {
    private const int BufferSize = 1024 * 1024;
    private readonly string _filePath;

    public Utf8StreamReaderImpl(string filePath)
    {
        _filePath = filePath;
    }

    public ValueTask Run()
    {
        var dictionary = new Dictionary<ulong, IntAccumulator>();
        using var stream = File.OpenRead(_filePath);

        var buffer = new byte[BufferSize];
        var span = buffer.AsSpan();

        int read = stream.Read(span);
        int offset = 0;

        while (read > 0) {
            var current = span.Slice(0, read + offset);
            //current = span.Slice(offset + read);

            while (current.Length > 0) {
                int eol = current.IndexOf((byte)'\n');
                if (eol == -1) {
                    current.CopyTo(span);
                    offset = current.Length;
                    break;
                }
                var line = current.Slice(0, eol);
                if (line.Length == 0) continue;
                ProcessLine(line, dictionary);
                current = current.Slice(eol + 1);
            }

            read = stream.Read(span.Slice(offset));
        }

        foreach (var accumulator in dictionary.Values.OrderBy(a => a.City))
        {
            Console.WriteLine($"{accumulator.City}: {accumulator.Min:F1}/{accumulator.Mean:F1}/{accumulator.Max:F1}");
        }


        return default;
    }

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
}