using Microsoft.VisualBasic;
using OneBRC.Shared;

namespace OneBRC;

public class StreamReaderImpl
{
    private readonly string _filePath;
    private readonly Lock _lock = new Lock();
    private static readonly Dictionary<string, int> IntFinder = Enumerable.Range(0, 200_000)
        .Select(r => (float)(r - 100_000) / 1000f)
        .Concat(new List<float> { -0.000f })
        .ToDictionary(r => r.ToString("0.000"), r => (int)(r * 1000));


    public StreamReaderImpl(string filePath)
    {
        _filePath = filePath;
    }

    public ValueTask Run()
    {
        var dictionary = new Dictionary<string, Accumulator>();
        //using var reader = File.OpenText(_filePath);

        foreach (var line in File.ReadLines(_filePath)) {
            /*
            var span = line.AsSpan();
            var split = span.IndexOf(';');
            var city = span.Slice(0, split).ToString();
            var value = span.Slice(split + 1);
            // lock (_lock) {
                if (!dictionary.TryGetValue(city, out var accumulator))
                {
                    dictionary[city] = accumulator = new Accumulator(city);
                }
                accumulator.Record(IntFinder[value.ToString()]);
            // }
            */
        };

        foreach (var accumulator in dictionary.Values.OrderBy(a => a.City))
        {
            Console.WriteLine($"{accumulator.City}: {accumulator.Min:F1}/{accumulator.Mean:F1}/{accumulator.Max:F1}");
        }

        return default;
    }
}