// MIT License

using Alyio.Extensions.Http.Logging;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    /// <param name="configureOptions">A delegate to configure the <see cref="HttpRawMessageLoggingOptions"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder" /> that can be used to configure the client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IHttpClientBuilder AddHttpRawMessageLogging(this IHttpClientBuilder builder, Action<HttpRawMessageLoggingOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.RemoveHttpRawMessageLogging();

        builder.Services.AddOptions<HttpRawMessageLoggingOptions>(builder.Name).Configure(options =>
        {
            configureOptions?.Invoke(options);
        });

#if NET8_0_OR_GREATER
        builder.ConfigureAdditionalHttpMessageHandlers((handlers, services) =>
        {
            HttpRawMessageLoggingHandler handler = BuildRawMessageLoggingHandler(services, builder.Name);
            handlers.Add(handler);
        });
#else
        builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(AddHttpRawMessageLoggingHandler);
        });
#endif

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
                if (action.Method.Name == nameof(AddHttpRawMessageLoggingHandler))
                {
                    options.HttpMessageHandlerBuilderActions.RemoveAt(i);
                }
            }
        });
#endif

        return builder;
    }

    private static HttpRawMessageLoggingHandler BuildRawMessageLoggingHandler(IServiceProvider services, string name)
    {
        HttpRawMessageLoggingOptions options = services.GetRequiredService<IOptionsSnapshot<HttpRawMessageLoggingOptions>>().Get(name);
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
        string clientName = string.IsNullOrEmpty(name) ? "Default" : name;
        string categoryName = options.CategoryName ?? $"System.Net.Http.HttpClient.{clientName}.{nameof(HttpRawMessageLoggingHandler)}";
        ILogger logger = loggerFactory.CreateLogger(categoryName);
        var handler = new HttpRawMessageLoggingHandler(logger, options);
        return handler;
    }

#if !NET8_0_OR_GREATER
    private static void AddHttpRawMessageLoggingHandler(HttpMessageHandlerBuilder b)
    {
        HttpRawMessageLoggingHandler handler = BuildRawMessageLoggingHandler(b.Services, b.Name!);
        b.AdditionalHandlers.Add(handler);
    }
#endif
}
