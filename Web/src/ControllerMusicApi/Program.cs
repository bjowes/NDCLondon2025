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
    // tracing.AddSource(Telemetry.Name);
    tracing.SetSampler(new TraceIdRatioBasedSampler(0.1)); // sample only 10% of requests
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
