using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

var httpClient = new HttpClient();

var minimalScenario = CreateWeatherForecastScenario(httpClient, 5001, "minimal_api"); 
var controllerScenario = CreateWeatherForecastScenario(httpClient, 6001, "controller"); 

NBomberRunner.RegisterScenarios(minimalScenario, controllerScenario).Run();

static ScenarioProps CreateWeatherForecastScenario(HttpClient httpClient, int port, string name) {
    return Scenario.Create(name, async context => 
    {
        var request = Http.CreateRequest("GET", $"https://localhost:{port}/weatherforecast").WithHeader("Accept", "application/json");
        var response = await Http.Send(httpClient, request);
        return response.IsError ? Response.Fail() : Response.Ok(statusCode: response.StatusCode, sizeBytes: response.SizeBytes);
    }).WithLoadSimulations(Simulation.Inject(
        rate: 100, 
        interval: TimeSpan.FromSeconds(1), 
        during: TimeSpan.FromSeconds(30))
    ).WithWarmUpDuration(TimeSpan.FromSeconds(30));
}