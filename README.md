# Alyio.Extensions.Http.Logging

[![Build Status](https://github.com/ousiax/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ousiax/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml)

**Alyio.Extensions.Http.Logging** extends the `HttpClientHandler` for logging raw HTTP request and response messages with detailed configuration options.

```sh
dotnet add package Alyio.Extensions.Http.Logging
```

To use the HTTP message logging functionality, use `IHttpClientBuilder.AddHttpRawMessageLogging` to add `HttpRawMessageLoggingHandler` as a handler into a specified `HttpClient`.

You can configure logging for all HTTP clients in your application using `ConfigureHttpClientDefaults`:

```cs
builder.Services.ConfigureHttpClientDefaults(builder =>
{
    builder.AddHttpRawMessageLogging(
        categoryName: "Alyio.Extensions.Http.Logging.Samples",
        logLevel: LogLevel.Information,
        ignoreRequestContent: false,
        ignoreResponseContent: false,
        ignoreRequestHeaders: ["User-Agent"],
        ignoreResponseHeaders: ["Date"]);
});
```

Or configure logging for a specific named HTTP client:

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
    .AddHttpRawMessageLogging(
        categoryName: "Alyio.Extensions.Http.Logging.Samples",
        logLevel: LogLevel.Information,
        ignoreRequestContent: false,
        ignoreResponseContent: false,
        ignoreRequestHeaders: ["User-Agent"],
        ignoreResponseHeaders: ["Date"]);

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

The `AddHttpRawMessageLogging` extension method provides the following configuration options:

- `categoryName`: The logger category name. If null, a default name will be used based on the client name.
- `logLevel`: The minimum log level for HTTP message logging. Defaults to `LogLevel.Information`.
- `ignoreRequestContent`: Whether to ignore the request content in logs. Defaults to true.
- `ignoreResponseContent`: Whether to ignore the response content in logs. Defaults to true.
- `ignoreRequestHeaders`: Headers to ignore in request logs.
- `ignoreResponseHeaders`: Headers to ignore in response logs.

Example output:

```console
$ dotnet run
info: System.Net.Http.HttpClient.IOpenWeatherMapService.HttpRawMessageLoggingHandler[0]
      Request-Queue: 1
info: System.Net.Http.HttpClient.IOpenWeatherMapService.HttpRawMessageLoggingHandler[0]
      Request-Message: 
      
      GET http://samples.openweathermap.org/data/2.5/weather?q=London,uk&appid=b1b15e88fa797225412429c1c50c122a1 HTTP/1.1
      
info: System.Net.Http.HttpClient.IOpenWeatherMapService.HttpRawMessageLoggingHandler[0]
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

## Migration from 3.x to 4.x

Version 4.0 introduces several breaking changes to improve the API and add new features. Here's how to migrate from version 3.x:

1. Update the package reference to version 4.0:
```xml
<PackageReference Include="Alyio.Extensions.Http.Logging" Version="4.0.0" />
```

2. Replace `AddLoggerHandler` with `AddHttpRawMessageLogging`:
```cs
// Old (3.x)
.AddLoggerHandler(ignoreRequestContent: false, ignoreResponseContent: false)

// New (4.x)
.AddHttpRawMessageLogging(
    categoryName: "Your.Category.Name",  // Optional
    logLevel: LogLevel.Information,      // Optional
    ignoreRequestContent: false,
    ignoreResponseContent: false,
    ignoreRequestHeaders: ["User-Agent"], // Optional
    ignoreResponseHeaders: ["Date"]);     // Optional
```

3. Update log category names in your logging configuration:
```cs
// Old (3.x)
.AddFilter("System.Net.Http.HttpClient.*.LoggerHandler", LogLevel.Information)

// New (4.x)
.AddFilter("System.Net.Http.HttpClient.*.HttpRawMessageLoggingHandler", LogLevel.Information)
```

Key changes in version 4.0:
- Renamed `LoggerHandler` to `HttpRawMessageLoggingHandler` for better clarity
- Renamed `AddLoggerHandler` to `AddHttpRawMessageLogging` to better describe its purpose
- Improved performance and memory usage
- Updated to target .NET 8.0
