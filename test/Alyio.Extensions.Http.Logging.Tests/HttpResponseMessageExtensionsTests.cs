// MIT License

using System.Net;
using System.Text;

namespace Alyio.Extensions.Http.Logging.Tests
{
    public class HttpResponseMessageExtensionsTests
    {
        [Fact]
        public async Task ReadRawMessageAsync_WhenDefaultParameters_ShouldIncludeContentAndHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains("HTTP/1.1 200 OK", raw);
            Assert.Contains("Content-Type: application/json", raw);
            Assert.Contains("HelloWorld", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenExplicitlyIncludeContent_ShouldIncludeContentAndHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            // Act
            string raw = await message.ReadRawMessageAsync(false);

            // Assert
            Assert.Contains("HTTP/1.1 200 OK", raw);
            Assert.Contains("Content-Type: application/json", raw);
            Assert.Contains("HelloWorld", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenIgnoreContent_ShouldExcludeContentButIncludeHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            // Act
            string raw = await message.ReadRawMessageAsync(true);

            // Assert
            Assert.Contains("HTTP/1.1 200 OK", raw);
            Assert.Contains("Content-Type: application/json", raw);
            Assert.DoesNotContain("HelloWorld", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenNoIgnoreHeaders_ShouldIncludeAllHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Found
            };
            message.Headers.Clear();
            message.Headers.Location = new Uri("http://localhost/foo/bar");
            message.Headers.Add("X-Custom-Header", "custom-value");

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains("HTTP/1.1 302 Found", raw);
            Assert.Contains("Location: http://localhost/foo/bar", raw);
            Assert.Contains("X-Custom-Header: custom-value", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenEmptyIgnoreHeaders_ShouldIncludeAllHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Found
            };
            message.Headers.Clear();
            message.Headers.Location = new Uri("http://localhost/foo/bar");
            message.Headers.Add("X-Custom-Header", "custom-value");

            // Act
            string raw = await message.ReadRawMessageAsync(ignoreHeaders: Array.Empty<string>());

            // Assert
            Assert.Contains("HTTP/1.1 302 Found", raw);
            Assert.Contains("Location: http://localhost/foo/bar", raw);
            Assert.Contains("X-Custom-Header: custom-value", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenIgnoreSingleHeader_ShouldExcludeSpecifiedHeader()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Found
            };
            message.Headers.Clear();
            message.Headers.Location = new Uri("http://localhost/foo/bar");
            message.Headers.Add("X-Custom-Header", "custom-value");

            // Act
            string raw = await message.ReadRawMessageAsync(ignoreHeaders: new string[] { "Location" });

            // Assert
            Assert.Contains("HTTP/1.1 302 Found", raw);
            Assert.DoesNotContain("Location: http://localhost/foo/bar", raw);
            Assert.Contains("X-Custom-Header: custom-value", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenIgnoreMultipleHeaders_ShouldExcludeSpecifiedHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Found
            };
            message.Headers.Clear();
            message.Headers.Location = new Uri("http://localhost/foo/bar");
            message.Headers.Add("X-Custom-Header", "custom-value");

            // Act
            string raw = await message.ReadRawMessageAsync(ignoreHeaders: new string[] { "Location", "X-Custom-Header" });

            // Assert
            Assert.Contains("HTTP/1.1 302 Found", raw);
            Assert.DoesNotContain("Location: http://localhost/foo/bar", raw);
            Assert.DoesNotContain("X-Custom-Header: custom-value", raw);
        }

        [Theory]
        [InlineData(200, "OK")]
        [InlineData(404, "Not Found")]
        [InlineData(500, "Internal Server Error")]
        public async Task ReadRawMessageAsync_WhenDifferentStatusCodes_ShouldIncludeStatusCodeAndReasonPhrase(int statusCode, string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)statusCode,
                ReasonPhrase = reasonPhrase
            };

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains($"HTTP/1.1 {statusCode} {reasonPhrase}", raw);
        }

        [Theory]
        [InlineData("application/json", "{\"key\":\"value\"}")]
        [InlineData("text/plain", "Hello World")]
        [InlineData("application/xml", "<root><item>value</item></root>")]
        public async Task ReadRawMessageAsync_WhenDifferentContentTypes_ShouldIncludeContentTypeAndContent(string contentType, string content)
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains($"Content-Type: {contentType}", raw);
            Assert.Contains(content, raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenContentHeadersPresent_ShouldIncludeContentHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };
            message.Content.Headers.ContentLength = 10;
            message.Content.Headers.ContentEncoding.Add("gzip");

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains("Content-Length: 10", raw);
            Assert.Contains("Content-Encoding: gzip", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenMultipleCustomHeaders_ShouldIncludeAllHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage();
            message.Headers.Clear();
            message.Headers.Add("X-Custom-Header1", "value1");
            message.Headers.Add("X-Custom-Header2", "value2");
            message.Headers.Add("X-Custom-Header3", "value3");

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains("X-Custom-Header1: value1", raw);
            Assert.Contains("X-Custom-Header2: value2", raw);
            Assert.Contains("X-Custom-Header3: value3", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenMultipleCustomHeadersAndIgnoreHeaders_ShouldExcludeSpecifiedHeaders()
        {
            // Arrange
            var message = new HttpResponseMessage();
            message.Headers.Clear();
            message.Headers.Add("X-Custom-Header1", "value1");
            message.Headers.Add("X-Custom-Header2", "value2");
            message.Headers.Add("X-Custom-Header3", "value3");

            // Act
            string raw = await message.ReadRawMessageAsync(ignoreHeaders: new string[] { "X-Custom-Header1", "X-Custom-Header3" });

            // Assert
            Assert.DoesNotContain("X-Custom-Header1: value1", raw);
            Assert.Contains("X-Custom-Header2: value2", raw);
            Assert.DoesNotContain("X-Custom-Header3: value3", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenContentIsNull_ShouldNotIncludeContentType()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                Content = null
            };

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.DoesNotContain("Content-Type", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenStreamContentWithoutMimeType_ShouldShowUnknownMimeType()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Stream Content"));
            var message = new HttpResponseMessage
            {
                Content = new StreamContent(stream)
            };

            // Act
            string raw = await message.ReadRawMessageAsync();

            // Assert
            Assert.Contains("[unknown]", raw);
        }

        [Fact]
        public async Task ReadRawMessageAsync_WhenCancellationTokenProvided_ShouldCompleteSuccessfully()
        {
            // Arrange
            var message = new HttpResponseMessage
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            // Act
            using var cts = new CancellationTokenSource();
            string raw = await message.ReadRawMessageAsync(cancellationToken: cts.Token);

            // Assert
            Assert.Contains("HelloWorld", raw);
        }

        [Theory]
        [InlineData("image/png")]
        [InlineData("image/jpeg")]
        [InlineData("audio/mpeg")]
        [InlineData("video/mp4")]
        [InlineData("application/octet-stream")]
        public async Task ReadRawMessageAsync_WithNonTextContent_ShouldIncludeBody(string contentType)
        {
            var message = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
            };
            message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains($"Content-Type: {contentType}", raw);
            Assert.Contains($"[{contentType}]", raw);
        }
    }
}
