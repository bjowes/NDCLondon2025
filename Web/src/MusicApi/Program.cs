using System.Diagnostics;
using System.Diagnostics.Metrics;
using MusicApi;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) {
    builder.AddServiceDefaults();
} else {
    builder.Logging.ClearProviders();
    builder.ConfigureOpenTelemetry();
}

builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing.AddSource(Telemetry.Name);
    tracing.SetSampler(new TraceIdRatioBasedSampler(0.1)); // sample only 10% of requests
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILoggerFactory loggerFactory, IMeterFactory meterFactory) =>
{
    var logger = loggerFactory.CreateLogger("WeatherForecast");
    var tempGauge = meterFactory.Create("MusicApi").CreateGauge<int>("max_temperature");
    logger.LogInformation("Getting weather forecast");
    try {
        /*
    var millisecondsDelay = Random.Shared.Next(10, 200);
    using (var activity = Telemetry.ActivitySource.StartActivity("FakeDelay", System.Diagnostics.ActivityKind.Client)) {
        activity?.AddTag("delay", millisecondsDelay);
        await Task.Delay(millisecondsDelay);
    }
    */

    if (Random.Shared.Next(100) == 50) {
        Activity.Current.AddTags(("foo", "fee"), ("fii", "fum"));
        throw new Exception("Fake exception");
    }

    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    tempGauge.Record(forecast.Max(x => x.TemperatureC));
    logger.LogInformation("Weather forecast returned");
    return forecast;
    } catch (Exception ex) {
        Activity.Current?.AddException(ex);
        Activity.Current?.SetStatus(Status.Error);
        logger.LogError(ex, "Error getting weather forecast");
        throw;
    }
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
