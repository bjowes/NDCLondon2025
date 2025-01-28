using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MusicApi;

public static class Telemetry {
    public static readonly string Name = "MusicApi";
    public static readonly ActivitySource ActivitySource = new(Name);
    //public static readonly Meter Meter = new(Name);
    //public static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>("poo");
    public static void AddTags(this Activity? activity, params ReadOnlySpan<(string, object)> tags) {
        if (activity is null) return;
        foreach (var (key, value) in tags) {
            activity.AddTag(key, value);
        }
    }
}