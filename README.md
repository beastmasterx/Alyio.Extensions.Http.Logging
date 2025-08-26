# Alyio.Extensions.Http.Logging

[![Build Status](https://github.com/alyiox/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/alyiox/Alyio.Extensions.Http.Logging/actions/workflows/ci.yml)
![NuGet Version](https://img.shields.io/nuget/v/alyio.extensions.http.logging)

**Alyio.Extensions.Http.Logging** is a .NET library that provides deep visibility into the HTTP and HTTPS traffic in applications. It extends `HttpClientHandler` to provide detailed, raw logging of HTTP requests and responses.

## Why Alyio.Extensions.Http.Logging?

In complex systems, understanding the exact data being sent and received over HTTP is crucial for debugging, auditing, and ensuring security. While tools like OpenTelemetry provide excellent high-level telemetry, they often don't capture the full raw payload of HTTP messages.

This library provides the ground-truth of HTTP communication, which is useful for:
- **Precision Debugging:** The exact headers and bodies of requests and responses can be inspected to quickly identify issues.
- **Auditing and Compliance:** A detailed log of all HTTP interactions can be maintained for security and compliance purposes.
- **Enhancing Existing Telemetry:** Existing telemetry solutions can be complemented with low-level details for drilling down into specific requests.

## Key Features

-   **Detailed HTTP Logging:** Capture and log the full, raw content of HTTP request and response messages, including headers and bodies.
-   **Highly Configurable:** Fine-tune logging with options to ignore specific headers or content, and control log levels.
-   **Sensitive Data Redaction:** Automatically redact sensitive information from headers (like `Authorization` tokens) to keep logs secure.
-   **Targeted Logging:** Apply logging to all `HttpClient` instances in an application or target specific named clients for granular control.
-   **Broad .NET Support:** Compatible with .NET 6.0, 8.0, 9.0 and 10.0.

## Installation

Install the package from NuGet:

```sh
dotnet add package Alyio.Extensions.Http.Logging
```

## Usage

To use the HTTP message logging functionality, use the `IHttpClientBuilder.AddHttpRawMessageLogging` extension method to add the `HttpRawMessageLoggingHandler` to the `HttpClient`.

### Configure Logging for All HTTP Clients

Logging can be configured for all HTTP clients in an application using `ConfigureHttpClientDefaults`:

```csharp
builder.Services.ConfigureHttpClientDefaults(builder =>
{
    builder.AddHttpRawMessageLogging(options =>
    {
        options.CategoryName = "MyCustomCategory";
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

Logging can also be configured for a specific named `HttpClient`:

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
        options.CategoryName = "MyOpenWeatherMapClient";
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
    .AddFilter("System.Net.Http.HttpClient", LogLevel.Information)
    .AddFilter("MyCustomCategory", LogLevel.Information)
    .AddFilter("MyOpenWeatherMapClient", LogLevel.Information);

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

## Configuration Options

The `AddHttpRawMessageLogging` extension method provides the following options:

-   `CategoryName`: The category name for the logger. Defaults to a name based on the `HttpClient` name.
-   `LogLevel`: The minimum log level for HTTP message logging. Defaults to `LogLevel.Information`.
-   `IgnoreRequestContent`: Whether to ignore the request content in logs. Defaults to `true`.
-   `IgnoreResponseContent`: Whether to ignore the response content in logs. Defaults to `true`.
-   `IgnoreRequestHeaders`: A collection of headers to ignore in request logs.
-   `IgnoreResponseHeaders`: A collection of headers to ignore in response logs.
-   `RedactRequestHeaders`: A collection of request header names to redact in logs. Defaults to `["Authorization"]`.
-   `RedactResponseHeaders`: A collection of response header names to redact in logs.

## Example Output

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

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the [MIT License](LICENSE).
