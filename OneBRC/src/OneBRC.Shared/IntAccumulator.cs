namespace OneBRC.Shared;

public struct IntAccumulator
{
    public string City { get; private set; }
    private int _total;
    private int _count;
    private int _min;
    private int _max;

    public IntAccumulator()
    {
        _min = int.MaxValue;
        _max = int.MinValue;
        City = null!;
    }

    public void SetCity(string city) {
        City = city;
    }

    public void Record(int value)
    {
        if (value < _min) _min = value;
        if (value > _max) _max = value;
        _total += value;
        ++_count;
    }
    
    public void Combine(IntAccumulator other)
    {
        _count += other._count;
        _total += other._total;
        if (other._min < _min) _min = other._min;
        if (other._max < _max) _max = other._max;
    }

    public float Mean => (_total / 1000f) / _count;
    public float Min => _min / 1000f;
    public float Max => _max / 1000f;
    public int Count => _count;
}