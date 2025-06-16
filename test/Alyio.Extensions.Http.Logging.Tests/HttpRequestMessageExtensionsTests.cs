// MIT License

using System.Net.Http.Headers;
using System.Text;

namespace Alyio.Extensions.Http.Logging.Tests
{
    public class HttpRequestMessageExtensionsTests
    {
        [Fact]
        public async Task ReadRawMessageAsync_WithContent_ShouldIncludeBody()
        {
            HttpRequestMessage message = new(HttpMethod.Post, "/foo/bar")
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains("HelloWorld", raw);
            Assert.Contains("Content-Type: application/json", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WithIgnoreContent_ShouldNotIncludeBody()
        {
            HttpRequestMessage message = new(HttpMethod.Post, "/foo/bar")
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            string raw = await message.ReadRawMessageAsync(ignoreContent: true);
            Assert.DoesNotContain("HelloWorld", raw);
            Assert.Contains("Content-Type: application/json", raw);
        }

        [Theory]
        [InlineData("Accept-Charset", "utf-8")]
        [InlineData("Accept", "application/json")]
        [InlineData("User-Agent", "TestClient/1.0")]
        [InlineData("Authorization", "Bearer token123")]
        [InlineData("X-Custom-Header", "custom-value")]
        public async Task ReadRawMessageAsync_WithSingleHeader_ShouldIncludeHeader(string headerName, string headerValue)
        {
            HttpRequestMessage message = new(HttpMethod.Get, "/foo/bar");
            message.Headers.Clear();
            message.Headers.TryAddWithoutValidation(headerName, headerValue);

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains($"{headerName}: {headerValue}", raw);
        }

        [Theory]
        [MemberData(nameof(GetIgnoreHeaders))]
        public async Task ReadRawMessageAsync_WithIgnoredHeaders_ShouldNotIncludeSpecifiedHeaders(string[] ignoreHeaders)
        {
            HttpRequestMessage message = new(HttpMethod.Get, "/foo/bar");
            message.Headers.Clear();
            message.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            message.Headers.UserAgent.Add(new ProductInfoHeaderValue("TestClient", "1.0"));
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token123");
            message.Headers.TryAddWithoutValidation("X-Custom-Header", "custom-value");

            string raw = await message.ReadRawMessageAsync(ignoreHeaders: ignoreHeaders);

            foreach (string header in ignoreHeaders)
            {
                Assert.DoesNotContain($"{header}:", raw);
            }
        }

        public static TheoryData<string[]> GetIgnoreHeaders()
        {
            return
            [
                ["Accept-Charset", "User-Agent"],
                ["Authorization", "X-Custom-Header"],
                ["Accept", "Content-Type"]
            ];
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task ReadRawMessageAsync_WithHttpMethod_ShouldIncludeMethod(string method)
        {
            HttpRequestMessage message = new(new HttpMethod(method), "/foo/bar");
            string raw = await message.ReadRawMessageAsync();
            Assert.Contains($"{method} /foo/bar", raw);
        }

        [Theory]
        [InlineData("application/json", "{\"key\":\"value\"}")]
        [InlineData("text/plain", "Hello World")]
        [InlineData("application/xml", "<root><item>value</item></root>")]
        public async Task ReadRawMessageAsync_WithContentType_ShouldIncludeContentTypeAndBody(string contentType, string content)
        {
            HttpRequestMessage message = new(HttpMethod.Post, "/foo/bar")
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains($"Content-Type: {contentType}", raw);
            Assert.Contains(content, raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WithNullContent_ShouldNotThrowException()
        {
            HttpRequestMessage message = new(HttpMethod.Get, "/foo/bar")
            {
                Content = null,
            };

            string raw = await message.ReadRawMessageAsync();
            Assert.DoesNotContain("Content-Type", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WithContentHeaders_ShouldIncludeContentHeaders()
        {
            HttpRequestMessage message = new(HttpMethod.Post, "/foo/bar")
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };
            message.Content.Headers.ContentLength = 10;
            message.Content.Headers.ContentEncoding.Add("gzip");

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains("Content-Length: 10", raw);
            Assert.Contains("Content-Encoding: gzip", raw);
        }
    }
}
