// MIT License

using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Alyio.Extensions.Http.Logging.E2ETests;

public class HttpRawMessageLoggingE2ETests
{
    private const string E2E_TEST_URL_GET = "https://httpbin.org/get";
    private const string E2E_TEST_URL_POST = "https://httpbin.org/post";
    private const string E2E_TEST_URL_HEADERS = "https://httpbin.org/headers";
    private const string E2E_TEST_URL_STATUS_404 = "https://httpbin.org/status/404";
    private const string E2E_TEST_URL_IMAGE_PNG = "https://httpbin.org/image/png";


    public class DefaultOptionsTests
    {
        [Theory]
        [InlineData(E2E_TEST_URL_GET)]
        public async Task ForGetRequest_ShouldLogRequestAndResponseWithoutBody(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();

            // Act
            await client.GetAsync(url);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            Assert.Collection(logs,
                l => Assert.StartsWith("Request-Queue", l.Message),
                l => Assert.StartsWith("Request-Message:", l.Message),
                l =>
                {
                    Assert.StartsWith("Response-Message:", l.Message);
                    Assert.DoesNotContain("\"url\"", l.Message); // httpbin /get response has "url"
                }
            );
        }

        [Theory]
        [InlineData(E2E_TEST_URL_POST)]
        public async Task ForPostRequest_ShouldLogRequestAndResponseWithoutBody(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();
            var content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            await client.PostAsync(url, content);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            Assert.Collection(logs,
                l => Assert.StartsWith("Request-Queue", l.Message),
                l =>
                {
                    Assert.StartsWith("Request-Message:", l.Message);
                    Assert.DoesNotContain("{\"key\":\"value\"}", l.Message);
                },
                l => Assert.StartsWith("Response-Message:", l.Message)
            );
        }

        [Theory]
        [InlineData(E2E_TEST_URL_HEADERS)]
        public async Task ForRequestWithAuthorizationHeader_ShouldRedactHeaderValue(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "my-secret-token");

            // Act
            await client.GetAsync(url);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));
            Assert.Contains("Authorization: ***", requestMessageLog.Message);
            Assert.DoesNotContain("my-secret-token", requestMessageLog.Message);
        }
    }

    public class CustomOptionsTests
    {
        [Theory]
        [InlineData(E2E_TEST_URL_POST)]
        public async Task WhenRequestContentLoggingEnabled_ShouldLogRequestBody(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreRequestContent = false;
            });
            var content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            await client.PostAsync(url, content);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            Assert.Collection(logs,
                l => Assert.StartsWith("Request-Queue", l.Message),
                l =>
                {
                    Assert.StartsWith("Request-Message:", l.Message);
                    Assert.Contains("{\"key\":\"value\"}", l.Message);
                },
                l => Assert.StartsWith("Response-Message:", l.Message)
            );
        }

        [Theory]
        [InlineData(E2E_TEST_URL_GET)]
        public async Task WhenResponseContentLoggingEnabled_ShouldLogResponseBody(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreResponseContent = false;
            });

            // Act
            await client.GetAsync(url);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            Assert.Collection(logs,
                l => Assert.StartsWith("Request-Queue", l.Message),
                l => Assert.StartsWith("Request-Message:", l.Message),
                l =>
                {
                    Assert.StartsWith("Response-Message:", l.Message);
                    Assert.Contains("\"url\":", l.Message); // httpbin /get response has "url"
                }
            );
        }

        [Theory]
        [InlineData(E2E_TEST_URL_HEADERS)]
        public async Task WhenHeaderIsIgnored_ShouldNotLogHeader(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreRequestHeaders = new string[] { "X-Test-Header" };
            });
            client.DefaultRequestHeaders.Add("X-Test-Header", "test-value");

            // Act
            await client.GetAsync(url);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));
            Assert.DoesNotContain("X-Test-Header", requestMessageLog.Message);
        }

        [Theory]
        [InlineData(E2E_TEST_URL_STATUS_404)]
        public async Task ForRequestToNonExistentEndpoint_ShouldLog404Response(string url)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var responseMessageLog = logs.First(l => l.Message.StartsWith("Response-Message:", StringComparison.Ordinal));
            Assert.Contains("HTTP/1.1 404 NOT FOUND", responseMessageLog.Message);
        }

        [Theory]
        [InlineData("MyCustomCategory")]
        public async Task WhenCustomCategoryIsUsed_ShouldLogUnderThatCategory(string customCategoryName)
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.CategoryName = customCategoryName;
            });

            // Act
            await client.GetAsync(E2E_TEST_URL_GET);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.Equals(customCategoryName, StringComparison.Ordinal) is true)
                .ToList();
            Assert.NotEmpty(logs);
            Assert.All(logs, l => Assert.Equal(customCategoryName, l.Category));
        }

        [Fact]
        public async Task ForNamedClients_ShouldLogUnderClientSpecificCategoryAndNotInterfere()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(b => b.AddFakeLogging());

            const string clientNameA = "ClientA";
            const string clientNameB = "ClientB";

            services.AddHttpClient(clientNameA).AddHttpRawMessageLogging();
            services.AddHttpClient(clientNameB).AddHttpRawMessageLogging();

            var serviceProvider = services.BuildServiceProvider();
            var loggerCollector = serviceProvider.GetFakeLogCollector();
            var clientA = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientNameA);
            var clientB = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientNameB);

            // Act
            await clientA.GetAsync(E2E_TEST_URL_GET);
            await clientB.GetAsync(E2E_TEST_URL_GET);

            // Assert
            var expectedCategoryA = $"System.Net.Http.HttpClient.{clientNameA}.{nameof(HttpRawMessageLoggingHandler)}";
            var expectedCategoryB = $"System.Net.Http.HttpClient.{clientNameB}.{nameof(HttpRawMessageLoggingHandler)}";

            var logsA = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.Equals(expectedCategoryA, StringComparison.Ordinal) is true)
                .ToList();
            var logsB = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.Equals(expectedCategoryB, StringComparison.Ordinal) is true)
                .ToList();

            Assert.NotEmpty(logsA);
            Assert.All(logsA, l => Assert.Equal(expectedCategoryA, l.Category));

            Assert.NotEmpty(logsB);
            Assert.All(logsB, l => Assert.Equal(expectedCategoryB, l.Category));

            // Ensure no cross-contamination
            Assert.DoesNotContain(logsA, l => l.Category?.Equals(expectedCategoryB, StringComparison.Ordinal) is true);
            Assert.DoesNotContain(logsB, l => l.Category?.Equals(expectedCategoryA, StringComparison.Ordinal) is true);
        }
    }

    private static (HttpClient, FakeLogCollector) CreateClientAndLogger(Action<HttpRawMessageLoggingOptions>? configureOptions = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddFakeLogging());

        var httpClientBuilder = services.AddHttpClient("Default");

        if (configureOptions != null)
        {
            httpClientBuilder.AddHttpRawMessageLogging(configureOptions);
        }
        else
        {
            httpClientBuilder.AddHttpRawMessageLogging();
        }

        var serviceProvider = services.BuildServiceProvider();
        var loggerCollector = serviceProvider.GetFakeLogCollector();
        var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("Default");

        return (client, loggerCollector);
    }
}
