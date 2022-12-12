
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Network.Ping;

var getStep = (string endpoint) => Step.Create($"get weather forecast: {endpoint}", async context =>
{
    var httpClient = new HttpClient();
    var address = new Uri($"http://localhost:5295/api/{endpoint}");
    var result = await httpClient.GetAsync(address);
    return result.StatusCode == System.Net.HttpStatusCode.OK ?
        Response.Ok() :
        Response.Fail();
});

var scenario1 = ScenarioBuilder.CreateScenario("get weather", getStep("Get"))
    .WithWarmUpDuration(TimeSpan.FromSeconds(5));

var scenario2 = ScenarioBuilder.CreateScenario("get cached weather", getStep("GetCached"))
    .WithWarmUpDuration(TimeSpan.FromSeconds(5));

var scenario3 = ScenarioBuilder.CreateScenario("get paged weather", getStep("GetPaged"))
    .WithWarmUpDuration(TimeSpan.FromSeconds(5));

var scenario4 = ScenarioBuilder.CreateScenario("get cached paged weather", getStep("GetPagedCache"))
    .WithWarmUpDuration(TimeSpan.FromSeconds(5));

NBomberRunner
    .RegisterScenarios(
        scenario1, 
        scenario2, 
        scenario3, 
        scenario4
        )
    .Run();