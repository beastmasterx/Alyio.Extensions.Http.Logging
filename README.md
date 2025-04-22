# Alyio.Extensions.Http.Logging

[![Build Status](https://github.com/ousiax/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ousiax/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml)

**Alyio..Extensions.Http.Logging** extends the `HttpClientHandler` for logging the HTTP request message and the HTTP response message.

```sh
dotnet add package Alyio.Extensions.Http.Logging
```

To use the `HttpClientHandler`, please use `IHttpClientBuilder.AddLoggerHandler` to add `LoggerHandler` as a handler into a specified `HttpClient`.

For example, the follow is a sample logging section.

```cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services
    .AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>(client =>
    {
        client.BaseAddress = new Uri("http://samples.openweathermap.org");
    })
    .AddLoggerHandler(ignoreRequestContent: false, ignoreResponseContent: false);

builder.Services.AddHostedService<OpenWeatherMapHostedService>();

builder.Logging
    .SetMinimumLevel(LogLevel.Warning)
    .AddFilter("System.Net.Http.HttpClient", LogLevel.Information);

IHost app = builder.Build();

await app.StartAsync();

sealed class OpenWeatherMapHostedService(IOpenWeatherMapService weatherMapService) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return weatherMapService.GetAsync();
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

sealed class OpenWeatherMapService(HttpClient client) : IOpenWeatherMapService
{
    public Task GetAsync()
    {
        return client.GetAsync("/data/2.5/weather?q=London,uk&appid=b1b15e88fa797225412429c1c50c122a1");
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
