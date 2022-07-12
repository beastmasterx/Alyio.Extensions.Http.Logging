using Alyio.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension mehtods for <see cref="IHttpClientBuilder"/>.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a logging delegate that will be used to log the http request and response message for a named <see cref="System.Net.Http.HttpClient" />.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder" /></param>
    /// <param name="categoryName"></param>
    /// <param name="logLevel">A <see cref="LogLevel"/> value indicates whether the logger is enalble; The default is <see cref="LogLevel.Information" />.</param>
    /// <param name="ignoreRequestContent">A <see cref="bool"/> value that indicates to ignore the request content. The default is true.</param>
    /// <param name="ignoreResponseContent">A <see cref="bool"/> value that indicates to ignore the response content. The default is true.</param>
    /// <param name="ignoreRequestHeaders">A <see cref="string"/> array to ignore the specified headers of <see cref="System.Net.Http.HttpRequestMessage.Headers"/>.</param>
    /// <param name="ignoreResponseHeaders">A <see cref="string"/> array to ignore the specified headers of <see cref="System.Net.Http.HttpRequestMessage.Headers"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder" /> that can be used to configure the client.</returns>
    public static IHttpClientBuilder AddLoggerHandler(
        this IHttpClientBuilder builder,
         string? categoryName = null,
         LogLevel logLevel = LogLevel.Information,
         bool ignoreRequestContent = true,
         bool ignoreResponseContent = true,
         string[]? ignoreRequestHeaders = null,
         string[]? ignoreResponseHeaders = null)
    {
        builder.Services.AddTransient<LoggerHandler>();
        var services = builder.Services.BuildServiceProvider();
        var handler = services.GetRequiredService<LoggerHandler>();

        handler.LoggerCategoryName = categoryName ?? $"System.Net.Http.HttpClient.{builder.Name}.{nameof(LoggerHandler)}";
        handler.LoggingOptions.LogLevel = logLevel;

        handler.LoggingOptions.IgnoreRequestContent = ignoreRequestContent;
        if (ignoreRequestHeaders is not null)
        {
            handler.LoggingOptions.IgnoreRequestHeaders = ignoreRequestHeaders;
        }

        if (ignoreResponseHeaders is not null)
        {
            handler.LoggingOptions.IgnoreResponseHeaders = ignoreResponseHeaders;
        }
        handler.LoggingOptions.IgnoreResponseContent = ignoreResponseContent;

        builder.AddHttpMessageHandler(h => { return handler; });

        return builder;
    }
}