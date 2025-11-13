// MIT License

using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Alyio.Extensions.Http.Logging.E2ETests;

public sealed class HttpRawMessageLoggingE2ETests
{
    private const string E2E_TEST_URL_GET = "/get";
    private const string E2E_TEST_URL_POST = "/post";
    private const string E2E_TEST_URL_HEADERS = "/headers";
    private const string E2E_TEST_URL_STATUS_404 = "/status/404";
    private const string E2E_TEST_URL_IMAGE_PNG = "/image/png";

    // a 1x1 PNG image (black pixel)
    private static readonly byte[] s_imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };


    public sealed class DefaultOptionsTests
    {
        [Fact]
        public async Task ForGetRequest_ShouldLogRequestAndResponseWithoutBody()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();

            // Act
            await client.GetAsync(E2E_TEST_URL_GET);

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

        [Fact]
        public async Task ForPostRequest_ShouldLogRequestAndResponseWithoutBody()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();
            var content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            await client.PostAsync(E2E_TEST_URL_POST, content);

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

        [Fact]
        public async Task ForRequestWithAuthorizationHeader_ShouldRedactHeaderValue()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "my-secret-token");

            // Act
            await client.GetAsync(E2E_TEST_URL_HEADERS);

            // Assert
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));
            Assert.Contains("Authorization: ***", requestMessageLog.Message);
            Assert.DoesNotContain("my-secret-token", requestMessageLog.Message);
        }
    }

    public sealed class CustomOptionsTests
    {
        [Fact]
        public async Task WhenRequestContentLoggingEnabled_ShouldLogRequestBody()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreRequestContent = false;
            });
            var content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync(E2E_TEST_URL_POST, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
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

        [Fact]
        public async Task WhenResponseContentLoggingEnabled_ShouldLogResponseBody()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreResponseContent = false;
            });

            // Act
            var response = await client.GetAsync(E2E_TEST_URL_GET);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
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

        [Fact]
        public async Task WhenHeaderIsIgnored_ShouldNotLogHeader()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
#pragma warning disable CA1861 // Avoid constant arrays as arguments
                options.IgnoreRequestHeaders = new string[] { "X-Test-Header" };
#pragma warning restore CA1861 // Avoid constant arrays as arguments
            });
            client.DefaultRequestHeaders.Add("X-Test-Header", "test-value");

            // Act
            var response = await client.GetAsync(E2E_TEST_URL_HEADERS);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));
            Assert.DoesNotContain("X-Test-Header", requestMessageLog.Message);
        }

        [Fact]
        public async Task ForRequestToNonExistentEndpoint_ShouldLog404Response()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger();

            // Act
            var response = await client.GetAsync(E2E_TEST_URL_STATUS_404);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var responseMessageLog = logs.First(l => l.Message.StartsWith("Response-Message:", StringComparison.Ordinal));
            Assert.Contains("HTTP/1.1 404 NOT FOUND", responseMessageLog.Message);
        }

        [Fact]
        public async Task WhenCustomCategoryIsUsed_ShouldLogUnderThatCategory()
        {
            // Arrange
            const string customCategoryName = "MyCustomCategory";
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.CategoryName = customCategoryName;
            });

            // Act
            var response = await client.GetAsync(E2E_TEST_URL_GET);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
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

            services.AddHttpClient(clientNameA, client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("HTTPBIN_URL") ?? "https://httpbin.org");
            }).AddHttpRawMessageLogging();
            services.AddHttpClient(clientNameB, client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("HTTPBIN_URL") ?? "https://httpbin.org");
            }).AddHttpRawMessageLogging();

            var serviceProvider = services.BuildServiceProvider();
            var loggerCollector = serviceProvider.GetFakeLogCollector();
            var clientA = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientNameA);
            var clientB = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientNameB);

            // Act
            var responseA = await clientA.GetAsync(E2E_TEST_URL_GET);
            var responseB = await clientB.GetAsync(E2E_TEST_URL_GET);

            // Assert
            Assert.True(responseA.IsSuccessStatusCode, responseA.ReasonPhrase);
            Assert.True(responseB.IsSuccessStatusCode, responseB.ReasonPhrase);
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

    public sealed class ImageBodyTests
    {
        [Fact]
        public async Task ForPostRequestWithImage_ShouldLogRequestAsImage()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreRequestContent = false;
                options.IgnoreResponseContent = false;
            });
            var content = new ByteArrayContent(s_imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            // Act
            var response = await client.PostAsync(E2E_TEST_URL_POST, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));

            Assert.Contains("[image/png]", requestMessageLog.Message);
        }

        [Fact]
        public async Task ForGetRequestWithImageResponse_ShouldLogResponseAsImage()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreResponseContent = false;
            });

            // Act
            var response = await client.GetAsync(E2E_TEST_URL_IMAGE_PNG);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var responseMessageLog = logs.First(l => l.Message.StartsWith("Response-Message:", StringComparison.Ordinal));
            Assert.Contains("[image/png]", responseMessageLog.Message);
        }
    }

    public sealed class FormBodyTests
    {
        [Fact]
        public async Task ForPostRequestWithFormUrlEncodedContent_ShouldLogRequestAsFormUrlEncoded()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreRequestContent = false;
                options.IgnoreResponseContent = false;
            });

            var content = new FormUrlEncodedContent(new Dictionary<string, string>{
                { "field1", "value1" },
                { "field2", "value2" },
            });

            // Act
            var response = await client.PostAsync(E2E_TEST_URL_POST, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);

            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));
            Assert.Contains("field1=value1", requestMessageLog.Message);
            Assert.Contains("field2=value2", requestMessageLog.Message);
        }

        [Fact]
        public async Task ForPostRequestWithMultipartFormDataContent_ShouldLogRequestAsMultipartFormData()
        {
            // Arrange
            var (client, loggerCollector) = CreateClientAndLogger(options =>
            {
                options.IgnoreRequestContent = false;
                options.IgnoreResponseContent = false;
            });

            var content = new MultipartFormDataContent
            {
                { new StringContent("value1"), "field1" },
                { new StringContent("value2"), "field2" },
            };
            var imageContent = new ByteArrayContent(s_imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(imageContent, "image", "image.png");

            // Act
            var response = await client.PostAsync(E2E_TEST_URL_POST, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);

            var logs = loggerCollector.GetSnapshot()
                .Where(l => l.Category?.EndsWith(nameof(HttpRawMessageLoggingHandler), StringComparison.Ordinal) is true)
                .ToList();
            var requestMessageLog = logs.First(l => l.Message.StartsWith("Request-Message:", StringComparison.Ordinal));
            Assert.Contains("name=field1", requestMessageLog.Message);
            Assert.Contains("name=field2", requestMessageLog.Message);
            Assert.Contains("name=image", requestMessageLog.Message);
            Assert.Contains("[image/png]", requestMessageLog.Message);
        }
    }

    private static (HttpClient, FakeLogCollector) CreateClientAndLogger(Action<HttpRawMessageLoggingOptions>? configureOptions = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddFakeLogging());

        var httpClientBuilder = services.AddHttpClient("Default", client =>
        {
            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("HTTPBIN_URL") ?? "https://httpbin.org");
        });

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
