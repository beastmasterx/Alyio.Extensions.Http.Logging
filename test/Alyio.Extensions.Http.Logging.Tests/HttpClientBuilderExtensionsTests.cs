// MIT License

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alyio.Extensions.Http.Logging.Tests;

public sealed class HttpClientBuilderExtensionsTests
{
    [Fact]
    public void AddHttpRawMessageLogging_Should_Have_LoggingHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        IHttpClientBuilder builder = services.AddHttpClient("TestClient");

        // Act
        builder.AddHttpRawMessageLogging();

        // Assert
        HttpMessageHandler handler = GetPrimaryHttpMessageHandler(builder);
        IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers = GetDelegatingHandlers(handler).OfType<HttpRawMessageLoggingHandler>();
        Assert.Single(loggingHandlers);
    }

    [Fact]
    public void AddHttpRawMessageLogging_Then_Remove_Should_Not_Have_LoggingHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        IHttpClientBuilder builder = services.AddHttpClient("TestClient");

        // Act
        builder.AddHttpRawMessageLogging();
        builder.RemoveHttpRawMessageLogging();

        // Assert
        HttpMessageHandler handler = GetPrimaryHttpMessageHandler(builder);
        IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers = GetDelegatingHandlers(handler).OfType<HttpRawMessageLoggingHandler>();
        Assert.Empty(loggingHandlers);
    }

    [Fact]
    public void AddHttpRawMessageLogging_Multiple_Times_Should_Only_Add_One_Handler()
    {
        // Arrange
        var services = new ServiceCollection();
        IHttpClientBuilder builder = services.AddHttpClient("TestClient");

        // Act
        builder.AddHttpRawMessageLogging();
        builder.AddHttpRawMessageLogging();

        // Assert
        HttpMessageHandler handler = GetPrimaryHttpMessageHandler(builder);
        IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers = GetDelegatingHandlers(handler).OfType<HttpRawMessageLoggingHandler>();
        Assert.Single(loggingHandlers);
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void ConfigureHttpClientDefaults_With_Add_And_Remove_Should_Behave_Correctly()
    {
        // Arrange
        var services = new ServiceCollection();
        IHttpClientBuilder builder1 = services.AddHttpClient("TestClient1");
        IHttpClientBuilder builder2 = services.AddHttpClient("TestClient2");
        IHttpClientBuilder builder3 = services.AddHttpClient(Options.DefaultName);

        // Act
        services.ConfigureHttpClientDefaults(b =>
        {
            b.AddHttpRawMessageLogging();
        });
        builder1.AddHttpRawMessageLogging(); // This should be idempotent
        builder2.RemoveHttpRawMessageLogging(); // This should remove the default one

        // Assert for TestClient1
        HttpMessageHandler handler1 = GetPrimaryHttpMessageHandler(builder1);
        IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers1 = GetDelegatingHandlers(handler1).OfType<HttpRawMessageLoggingHandler>();
        Assert.Single(loggingHandlers1);

        // Assert for TestClient2
        HttpMessageHandler handler2 = GetPrimaryHttpMessageHandler(builder2);
        IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers2 = GetDelegatingHandlers(handler2).OfType<HttpRawMessageLoggingHandler>();
        Assert.Empty(loggingHandlers2);

        // Assert for Default Client
        HttpMessageHandler handler3 = GetPrimaryHttpMessageHandler(builder3);
        IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers3 = GetDelegatingHandlers(handler3).OfType<HttpRawMessageLoggingHandler>();
        Assert.Single(loggingHandlers3);
    }
#endif

    private static HttpMessageHandler GetPrimaryHttpMessageHandler(IHttpClientBuilder builder)
    {
        ServiceProvider services = builder.Services.BuildServiceProvider();
        IHttpMessageHandlerFactory handlerFactory = services.GetRequiredService<IHttpMessageHandlerFactory>();
        return handlerFactory.CreateHandler(builder.Name);
    }

    private static IEnumerable<DelegatingHandler> GetDelegatingHandlers(HttpMessageHandler handler)
    {
        var current = handler as DelegatingHandler;
        while (current != null)
        {
            yield return current;
            current = current.InnerHandler as DelegatingHandler;
        }
    }
}
