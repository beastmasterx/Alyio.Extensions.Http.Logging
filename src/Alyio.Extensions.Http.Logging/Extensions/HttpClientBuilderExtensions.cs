// MIT License

using Alyio.Extensions.Http.Logging;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for <see cref="IHttpClientBuilder"/> that provide HTTP message logging functionality.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds raw HTTP message logging to the <see cref="HttpClient" /> pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder" />.</param>
    /// <param name="categoryName">The logger category name. If null, a default name will be used based on the client name.</param>
    /// <param name="logLevel">The <see cref="LogLevel"/> to use for logging. Defaults to <see cref="LogLevel.Information" />.</param>
    /// <param name="ignoreRequestContent">Whether to ignore the request content in logs. Defaults to true.</param>
    /// <param name="ignoreResponseContent">Whether to ignore the response content in logs. Defaults to true.</param>
    /// <param name="ignoreRequestHeaders">Headers to ignore in request logs.</param>
    /// <param name="ignoreResponseHeaders">Headers to ignore in response logs.</param>
    /// <returns>An <see cref="IHttpClientBuilder" /> that can be used to configure the client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IHttpClientBuilder AddHttpRawMessageLogging(
        this IHttpClientBuilder builder,
        string? categoryName = null,
        LogLevel logLevel = LogLevel.Information,
        bool ignoreRequestContent = true,
        bool ignoreResponseContent = true,
        string[]? ignoreRequestHeaders = null,
        string[]? ignoreResponseHeaders = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.RemoveHttpRawMessageLogging();

        builder.Services.AddTransient<HttpRawMessageLoggingHandler>();
        builder.Services.AddOptions<HttpMessageLoggingOptions>().Configure(options =>
        {
            options.CategoryName = categoryName ?? $"System.Net.Http.HttpClient.{builder.Name}.{nameof(HttpRawMessageLoggingHandler)}";
            options.Level = logLevel;
            options.IgnoreRequestContent = ignoreRequestContent;
            options.IgnoreResponseContent = ignoreResponseContent;

            if (ignoreRequestHeaders != null)
            {
                options.IgnoreRequestHeaders = ignoreRequestHeaders;
            }

            if (ignoreResponseHeaders != null)
            {
                options.IgnoreResponseHeaders = ignoreResponseHeaders;
            }
        });

        builder.AddHttpMessageHandler<HttpRawMessageLoggingHandler>();

        return builder;
    }

    /// <summary>
    /// Removes the <see cref="HttpRawMessageLoggingHandler"/> from the <see cref="HttpClient"/> pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder" />.</param>
    /// <returns>An <see cref="IHttpClientBuilder" /> that can be used to configure the client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IHttpClientBuilder RemoveHttpRawMessageLogging(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#if NET8_0_OR_GREATER
        _ = builder.ConfigureAdditionalHttpMessageHandlers(static (handlers, _) =>
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                if (handlers[i] is HttpRawMessageLoggingHandler)
                {
                    handlers.RemoveAt(i);
                }
            }
        });
#else
        _ = builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
        {
            for (int i = options.HttpMessageHandlerBuilderActions.Count - 1; i >= 0; i--)
            {
                Action<HttpMessageHandlerBuilder> action = options.HttpMessageHandlerBuilderActions[i];
                if (action.Target?.GetType()?.GetGenericArguments()?.FirstOrDefault() == typeof(HttpRawMessageLoggingHandler))
                {
                    options.HttpMessageHandlerBuilderActions.RemoveAt(i);
                }
            }
        });
#endif

        return builder;
    }
}
