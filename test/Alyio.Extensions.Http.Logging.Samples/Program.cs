using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>(
            configureClient => configureClient.BaseAddress = new Uri("http://samples.openweathermap.org"))
            .AddLoggerHandler(ignoreRequestContent: false, ignoreResponseContent: false);

        services.AddHostedService<OpenWeatherMapHostedService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddFilter("System.Net.Http.HttpClient.IOpenWeatherMapService.LoggingHandler", LogLevel.Information);
    })
    .UseConsoleLifetime();

await host.RunConsoleAsync();


sealed class OpenWeatherMapHostedService : IHostedService
{
    private readonly IOpenWeatherMapService _openWeatherMap;

    public OpenWeatherMapHostedService(IOpenWeatherMapService weatherMapService)
    {
        _openWeatherMap = weatherMapService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _openWeatherMap.GetAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

internal interface IOpenWeatherMapService
{
    Task GetAsync();
}

sealed class OpenWeatherMapService : IOpenWeatherMapService
{
    private readonly HttpClient _client;

    public OpenWeatherMapService(HttpClient client)
    {
        _client = client;
    }

    public Task GetAsync()
    {
        return _client.GetAsync("/data/2.5/weather?q=London,uk&appid=b1b15e88fa797225412429c1c50c122a1");
    }
}