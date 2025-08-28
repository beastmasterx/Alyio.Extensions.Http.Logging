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
            var options = services.GetRequiredService<IOptionsSnapshot<HttpRawMessageLoggingOptions>>().Get(builder.Name);
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var clientName = string.IsNullOrEmpty(builder.Name) ? "Default" : builder.Name;
            var categoryName = options.CategoryName ?? $"System.Net.Http.HttpClient.{clientName}.{nameof(HttpRawMessageLoggingHandler)}";
            var logger = loggerFactory.CreateLogger(categoryName);
            var handler = new HttpRawMessageLoggingHandler(logger, options);
            handlers.Add(handler);
        });
#else
        builder.Services.AddTransient<IHttpMessageHandlerBuilderFilter, HttpRawMessageLoggingFilter>();
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

        // for (int i = builder.Services.Count - 1; i >= 0; i--)
        // {
        //     ServiceDescriptor descriptor = builder.Services[i];
        //     if (descriptor.ImplementationType == typeof(HttpRawMessageLoggingFilter))
        //     {
        //         builder.Services.RemoveAt(i);
        //     }
        // }
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

    private sealed class HttpRawMessageLoggingFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                HttpRawMessageLoggingOptions options = builder.Services.GetRequiredService<IOptionsSnapshot<HttpRawMessageLoggingOptions>>().Get(builder.Name);
                ILoggerFactory loggerFactory = builder.Services.GetRequiredService<ILoggerFactory>();
                string clientName = string.IsNullOrEmpty(builder.Name) ? "Default" : builder.Name;
                string categoryName = options.CategoryName ?? $"System.Net.Http.HttpClient.{clientName}.{nameof(HttpRawMessageLoggingHandler)}";
                ILogger logger = loggerFactory.CreateLogger(categoryName);

                var handler = new HttpRawMessageLoggingHandler(logger, options);
                builder.AdditionalHandlers.Add(handler);

                next(builder);
            };
        }
    }
}
