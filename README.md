# Alyio.Extensions.Http.Logging

[![Build Status](https://github.com/ousiax/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ousiax/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml)

**Alyio.Extensions.Http.Logging** is a .NET library that extends `HttpClientHandler` to provide logging of raw HTTP request and response messages. It offers detailed configuration options to customize the logging output.

## What's New in 4.2.0

-   **Header Redaction**: You can now redact sensitive information from request and response headers in the logs. By default, the `Authorization` header is redacted from request headers.

## Features

-   Log raw HTTP request and response messages.
-   Configure logging for all `HttpClient` instances or specific named clients.
-   Customize logging with options to ignore request/response content and headers.
-   Redact sensitive information from headers.
-   Supports .NET 6.0, 8.0, 9.0 and 10.0.

## Installation

Install the package from NuGet:

```sh
dotnet add package Alyio.Extensions.Http.Logging
```

## Usage

To use the HTTP message logging functionality, use the `IHttpClientBuilder.AddHttpRawMessageLogging` extension method to add the `HttpRawMessageLoggingHandler` to your `HttpClient`.

### Configure Logging for All HTTP Clients

You can configure logging for all HTTP clients in your application using `ConfigureHttpClientDefaults`:

```csharp
builder.Services.ConfigureHttpClientDefaults(builder =>
{
    builder.AddHttpRawMessageLogging(options =>
    {
        options.LogLevel = LogLevel.Information;
        options.IgnoreRequestContent = false;
        options.IgnoreResponseContent = false;
        options.IgnoreRequestHeaders = new[] { "User-Agent" };
        options.IgnoreResponseHeaders = new[] { "Date" };
        options.RedactRequestHeaders = new[] { "Authorization", "X-Api-Key" };
    });
});
```

### Configure Logging for a Specific Named HTTP Client

You can also configure logging for a specific named `HttpClient`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services
    .AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>(client =>
    {
        client.BaseAddress = new Uri("http://samples.openweathermap.org");
    })
    .AddHttpRawMessageLogging(options =>
    {
        options.LogLevel = LogLevel.Information;
        options.IgnoreRequestContent = false;
        options.IgnoreResponseContent = false;
        options.IgnoreRequestHeaders = new[] { "User-Agent" };
        options.IgnoreResponseHeaders = new[] { "Date" };
        options.RedactRequestHeaders = new[] { "Authorization", "X-Api-Key" };
    });

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

### Configuration Options

The `AddHttpRawMessageLogging` extension method provides the following options:

-   `LogLevel`: The minimum log level for HTTP message logging. Defaults to `LogLevel.Information`.
-   `IgnoreRequestContent`: Whether to ignore the request content in logs. Defaults to `true`.
-   `IgnoreResponseContent`: Whether to ignore the response content in logs. Defaults to `true`.
-   `IgnoreRequestHeaders`: A collection of headers to ignore in request logs.
-   `IgnoreResponseHeaders`: A collection of headers to ignore in response logs.
-   `RedactRequestHeaders`: A collection of request header names to redact in logs. Defaults to `["Authorization"]`.
-   `RedactResponseHeaders`: A collection of response header names to redact in logs.

### Example Output

```console
$ dotnet run
info: System.Net.Http.HttpClient.IOpenWeatherMapService.HttpRawMessageLoggingHandler[0]
      Request-Queue: 1
info: System.Net.Http.HttpClient.IOpenWeatherMapService.HttpRawMessageLoggingHandler[0]
      Request-Message:

      GET http://samples.openweathermap.org/data/2.5/weather?q=London,uk&appid=b1b15e88fa797225412429c1c50c122a1 HTTP/1.1
      Authorization: ***

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

1.  Update the package reference to version 4.0 or later:

    ```xml
    <PackageReference Include="Alyio.Extensions.Http.Logging" Version="4.2.0" />
    ```

2.  Replace `AddLoggerHandler` with `AddHttpRawMessageLogging`:

    ```csharp
    // Old (3.x)
    .AddLoggerHandler(ignoreRequestContent: false, ignoreResponseContent: false)

    // New (4.x)
    .AddHttpRawMessageLogging(options =>
    {
        options.IgnoreRequestContent = false;
        options.IgnoreResponseContent = false;
        options.IgnoreRequestHeaders = new[] { "User-Agent" };
        options.IgnoreResponseHeaders = new[] { "Date" };
    });
    ```

3.  Update log category names in your logging configuration:

    ```csharp
    // Old (3.x)
    .AddFilter("System.Net.Http.HttpClient.*.LoggerHandler", LogLevel.Information)

    // New (4.x)
    .AddFilter("System.Net.Http.HttpClient.*.HttpRawMessageLoggingHandler", LogLevel.Information)
    ```

### Key Changes in Version 4.0

-   Renamed `LoggerHandler` to `HttpRawMessageLoggingHandler` for better clarity.
-   Renamed `AddLoggerHandler` to `AddHttpRawMessageLogging` to better describe its purpose.
-   Improved performance and memory usage.
-   Updated to target .NET 8.0.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the [MIT License](LICENSE).
