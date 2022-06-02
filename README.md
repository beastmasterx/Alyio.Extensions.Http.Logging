# Alyio.Extensions.Http.Logging

[![Build Status](https://github.com/qqbuby/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/qqbuby/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml)

**Alyio..Extensions.Http.Logging** extends the `HttpClientHandler` for logging the HTTP request message and the HTTP response message.

To use the `HttpClientHandler`, please use `IHttpClientBuilder.AddLoggingHandler` to add `LoggingHandler` as an handler into a specified `HttpClient`.

For example, the follow is a sample logging section.

```cs
using Alyio.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>(
            configureClient => configureClient.BaseAddress = new Uri("http://samples.openweathermap.org"))
            .AddLoggingHandler(ignoreRequestContent: false, ignoreResponseContent: false);

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
```

```console
$ dotnet run
info: System.Net.Http.HttpClient.IOpenWeatherMapService.LoggerHandler[0]
      Request-Queue: 1
info: System.Net.Http.HttpClient.IOpenWeatherMapService.LoggerHandler[0]
      Request-Message: 
      
      GET http://samples.openweathermap.org/data/2.5/weather?q=London,uk&appid=b1b15e88fa797225412429c1c50c122a1 HTTP/1.1
      
info: System.Net.Http.HttpClient.IOpenWeatherMapService.LoggerHandler[0]
      Response-Message: 4732ms
      
      HTTP/1.1 200 OK
      Server: openresty/1.9.7.1
      Date: Thu, 02 Jun 2022 15:42:01 GMT
      Transfer-Encoding: chunked
      Connection: keep-alive
      X-Frame-Options: SAMEORIGIN
      X-XSS-Protection: 1; mode=block
      X-Content-Type-Options: nosniff
      ETag: W/"e70c27085ed41de5321252b16c9582fe"
      Cache-Control: must-revalidate, max-age=0, private
      X-Request-ID: ab78dbeb-90bb-49d9-8812-984205851f0f
      X-Runtime: 0.001029
      Content-Type: application/json; charset=utf-8
      
      {"coord":{"lon":-0.13,"lat":51.51},"weather":[{"id":300,"main":"Drizzle","description":"light intensity drizzle","icon":"09d"}],"base":"stations","main":{"temp":280.32,"pressure":1012,"humidity":81,"temp_min":279.15,"temp_max":281.15},"visibility":10000,"wind":{"speed":4.1,"deg":80},"clouds":{"all":90},"dt":1485789600,"sys":{"type":1,"id":5091,"message":0.0103,"country":"GB","sunrise":1485762037,"sunset":1485794875},"id":2643743,"name":"London","cod":200}
^C
```
