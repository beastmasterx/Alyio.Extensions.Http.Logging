// MIT License

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alyio.Extensions.Http.Logging.Tests;

/// <summary>
/// Tests for <see cref="HttpClientBuilderExtensions"/> functionality.
/// </summary>
[Trait("Category", "HttpClientBuilder.Extensions")]
public sealed class HttpClientBuilderExtensionsTests
{
    /// <summary>
    /// Tests for adding HTTP raw message logging functionality.
    /// </summary>
    [Trait("Category", "HttpClientBuilder.Extensions.AddHttpRawMessageLogging")]
    public class AddHttpRawMessageLoggingTests
    {
        /// <summary>
        /// Verifies that calling AddHttpRawMessageLogging multiple times only adds one handler,
        /// preventing duplicate handlers in the pipeline.
        /// </summary>
        [Fact]
        public void WhenCalledMultipleTimes_ShouldOnlyAddOneHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder = services.AddHttpClient("TestClient");

            // Act
            builder.AddHttpRawMessageLogging();
            builder.AddHttpRawMessageLogging();

            // Assert
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers = GetDelegatingHandlers(builder).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(loggingHandlers);
        }

        /// <summary>
        /// Verifies that adding HTTP raw message logging works with different client name scenarios.
        /// </summary>
        [Theory]
        [InlineData("TestClient")]
        [InlineData("")]
        [InlineData("Client-With-Special-Chars")]
        [InlineData("Client_With_Underscores")]
        [InlineData("Client.With.Dots")]
        [InlineData("123NumericClient")]
        public void WithDifferentClientNames_ShouldAddHandler(string clientName)
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder = services.AddHttpClient(clientName);

            // Act
            builder.AddHttpRawMessageLogging();

            // Assert
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers = GetDelegatingHandlers(builder).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(loggingHandlers);
        }

        /// <summary>
        /// Verifies that adding logging to multiple HTTP clients with different names does not interfere.
        /// </summary>
        [Theory]
        [InlineData("Client1", "Client2")]
        [InlineData("", "DefaultClient")]
        [InlineData("ApiClient", "WebClient")]
        [InlineData("Test-Client-1", "Test_Client_2")]
        [InlineData("123Client", "Client456")]
        [InlineData("TestClient1", "TestClient2")] // Original test case
        public void WithMultipleClients_ShouldNotInterfere(string client1, string client2)
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder1 = services.AddHttpClient(client1);
            IHttpClientBuilder builder2 = services.AddHttpClient(client2);

            // Act
            builder1.AddHttpRawMessageLogging();
            builder2.AddHttpRawMessageLogging();

            // Assert
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers1 = GetDelegatingHandlers(builder1).OfType<HttpRawMessageLoggingHandler>();
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers2 = GetDelegatingHandlers(builder2).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(loggingHandlers1);
            Assert.Single(loggingHandlers2);
        }

        /// <summary>
        /// Verifies that adding HTTP raw message logging with null builder throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void WhenBuilderIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IHttpClientBuilder? builder = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                builder!.AddHttpRawMessageLogging());
            Assert.Equal("builder", exception.ParamName);
        }

        /// <summary>
        /// Verifies that adding HTTP raw message logging without options uses default configuration.
        /// </summary>
        [Fact]
        public void WithoutOptions_ShouldUseDefaultConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = services.AddHttpClient("TestClient");

            // Act
            builder.AddHttpRawMessageLogging();

            // Assert
            var handler = GetDelegatingHandlers(builder).OfType<HttpRawMessageLoggingHandler>().Single();
            Assert.NotNull(handler);

            // Verify default options are applied
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpRawMessageLoggingOptions>>().Get(builder.Name);
            Assert.Null(options.CategoryName); // Default is null, will be computed
            Assert.Equal(LogLevel.Information, options.Level);
            Assert.True(options.IgnoreRequestContent);
            Assert.True(options.IgnoreResponseContent);
            Assert.Empty(options.IgnoreRequestHeaders);
            Assert.Contains("Authorization", options.RedactRequestHeaders);
            Assert.Empty(options.IgnoreResponseHeaders);
            Assert.Empty(options.RedactResponseHeaders);
        }

        /// <summary>
        /// Verifies that adding HTTP raw message logging with various options configurations works correctly.
        /// </summary>
        [Theory]
        [InlineData("CustomCategory", LogLevel.Debug, false, false, new[] { "User-Agent" }, new[] { "Authorization", "X-API-Key" })]
        [InlineData("AnotherCategory", LogLevel.Warning, true, false, new string[0], new[] { "X-Secret" })]
        [InlineData(null, LogLevel.Error, false, true, new[] { "Accept", "Content-Type" }, new string[0])]
        [InlineData("TestLogger", LogLevel.Information, true, true, new[] { "User-Agent", "Accept" }, new[] { "Authorization", "Bearer" })]
        public void WithOptionsConfiguration_ShouldConfigureHandlerCorrectly(
            string? categoryName,
            LogLevel logLevel,
            bool ignoreRequestContent,
            bool ignoreResponseContent,
            string[] ignoreHeaders,
            string[] redactHeaders)
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = services.AddHttpClient("TestClient");

            // Act
            builder.AddHttpRawMessageLogging(options =>
            {
                options.CategoryName = categoryName;
                options.Level = logLevel;
                options.IgnoreRequestContent = ignoreRequestContent;
                options.IgnoreResponseContent = ignoreResponseContent;
                options.IgnoreRequestHeaders = ignoreHeaders;
                options.RedactRequestHeaders = redactHeaders;
            });

            // Assert
            var handler = GetDelegatingHandlers(builder).OfType<HttpRawMessageLoggingHandler>().Single();
            Assert.NotNull(handler);

            // Verify options are applied
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpRawMessageLoggingOptions>>().Get(builder.Name);
            Assert.Equal(categoryName, options.CategoryName);
            Assert.Equal(logLevel, options.Level);
            Assert.Equal(ignoreRequestContent, options.IgnoreRequestContent);
            Assert.Equal(ignoreResponseContent, options.IgnoreResponseContent);
            Assert.Equal(ignoreHeaders, options.IgnoreRequestHeaders);
            Assert.Equal(redactHeaders, options.RedactRequestHeaders);
        }
    }

    /// <summary>
    /// Tests for removing HTTP raw message logging functionality.
    /// </summary>
    [Trait("Category", "HttpClientBuilder.Extensions.RemoveHttpRawMessageLogging")]
    public class RemoveHttpRawMessageLoggingTests
    {
        /// <summary>
        /// Verifies that removing HTTP raw message logging removes the logging handler
        /// from the HTTP client pipeline.
        /// </summary>
        [Fact]
        public void WhenCalled_ShouldRemoveLoggingHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder = services.AddHttpClient("TestClient");

            // Act
            builder.AddHttpRawMessageLogging();
            builder.RemoveHttpRawMessageLogging();

            // Assert
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers = GetDelegatingHandlers(builder).OfType<HttpRawMessageLoggingHandler>();
            Assert.Empty(loggingHandlers);
        }

        /// <summary>
        /// Verifies that removing logging from one client does not affect other clients,
        /// ensuring proper isolation between different client configurations.
        /// </summary>
        [Fact]
        public void ForOneClient_ShouldNotAffectOtherClients()
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder1 = services.AddHttpClient("TestClient1");
            IHttpClientBuilder builder2 = services.AddHttpClient("TestClient2");

            // Act
            builder1.AddHttpRawMessageLogging();
            builder2.AddHttpRawMessageLogging();
            builder2.RemoveHttpRawMessageLogging();

            // Assert
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers1 = GetDelegatingHandlers(builder1).OfType<HttpRawMessageLoggingHandler>();
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers2 = GetDelegatingHandlers(builder2).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(loggingHandlers1);
            Assert.Empty(loggingHandlers2);
        }

        /// <summary>
        /// Verifies that removing HTTP raw message logging with null builder throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void WhenBuilderIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IHttpClientBuilder? builder = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                builder!.RemoveHttpRawMessageLogging());
            Assert.Equal("builder", exception.ParamName);
        }

        /// <summary>
        /// Verifies that removing logging when no logging handler exists does not cause errors.
        /// </summary>
        [Fact]
        public void WhenNoLoggingHandlerExists_ShouldNotCauseErrors()
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder = services.AddHttpClient("TestClient");

            // Act & Assert
            var exception = Record.Exception(() => builder.RemoveHttpRawMessageLogging());
            Assert.Null(exception);
        }
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Tests for ConfigureHttpClientDefaults functionality (NET 8.0+ only).
    /// </summary>
    [Trait("Category", "HttpClientBuilder.Extensions.ConfigureHttpClientDefaults")]
    public class ConfigureHttpClientDefaultsTests
    {
        /// <summary>
        /// Verifies that ConfigureHttpClientDefaults adds logging handler to all clients
        /// including the default client.
        /// </summary>
        [Fact]
        public void WhenConfigured_ShouldAddLoggingHandlerToAllClients()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.ConfigureHttpClientDefaults(b => { b.AddHttpRawMessageLogging(); });
            IHttpClientBuilder builder0 = services.AddHttpClient(Options.DefaultName); // default
            IHttpClientBuilder builder1 = services.AddHttpClient("TestClient1");
            IHttpClientBuilder builder2 = services.AddHttpClient("TestClient2");

            // Assert
            IEnumerable<DelegatingHandler> handlers0 = GetDelegatingHandlers(builder0).OfType<HttpRawMessageLoggingHandler>();
            IEnumerable<DelegatingHandler> handlers1 = GetDelegatingHandlers(builder1).OfType<HttpRawMessageLoggingHandler>();
            IEnumerable<DelegatingHandler> handlers2 = GetDelegatingHandlers(builder2).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(handlers0);
            Assert.Single(handlers1);
            Assert.Single(handlers2);
        }

        /// <summary>
        /// Verifies that ConfigureHttpClientDefaults works correctly with individual client
        /// add and remove operations.
        /// </summary>
        [Fact]
        public void WithIndividualClientOperations_ShouldBehaveCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            IHttpClientBuilder builder0 = services.AddHttpClient(Options.DefaultName); // default
            IHttpClientBuilder builder1 = services.AddHttpClient("TestClient1");
            IHttpClientBuilder builder2 = services.AddHttpClient("TestClient2");

            // Act
            services.ConfigureHttpClientDefaults(b =>
            {
                b.AddHttpRawMessageLogging(); // builder0
            });
            builder1.AddHttpRawMessageLogging();
            builder2.AddHttpRawMessageLogging();
            builder2.RemoveHttpRawMessageLogging();

            // Assert for Default Client
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers0 = GetDelegatingHandlers(builder0).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(loggingHandlers0);

            // Assert for TestClient1
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers1 = GetDelegatingHandlers(builder1).OfType<HttpRawMessageLoggingHandler>();
            Assert.Single(loggingHandlers1);

            // Assert for TestClient2
            IEnumerable<HttpRawMessageLoggingHandler> loggingHandlers2 = GetDelegatingHandlers(builder2).OfType<HttpRawMessageLoggingHandler>();
            Assert.Empty(loggingHandlers2);
        }
    }
#endif

    /// <summary>
    /// Helper method to get all delegating handlers from an HTTP client builder.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>Collection of delegating handlers in the pipeline.</returns>
    private static IEnumerable<DelegatingHandler> GetDelegatingHandlers(IHttpClientBuilder builder)
    {
        ServiceProvider services = builder.Services.BuildServiceProvider();
        IHttpMessageHandlerFactory handlerFactory = services.GetRequiredService<IHttpMessageHandlerFactory>();
        HttpMessageHandler handler = handlerFactory.CreateHandler(builder.Name);
        var current = handler as DelegatingHandler;
        while (current != null)
        {
            yield return current;
            current = current.InnerHandler as DelegatingHandler;
        }
    }
}
