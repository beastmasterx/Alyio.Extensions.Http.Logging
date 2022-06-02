using System.Net.Http.Headers;
using System.Text;

namespace Alyio.Extensions.Http.Logging.Tests
{
    public class HttpRequestMessageExtensionsTests
    {
        [Fact]
        public async Task ReadRawMessageAsync_body_Test()
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "/foo/bar")
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            var raw = await message.ReadRawMessageAsync();
            Assert.Contains("HelloWorld", raw);

            var raw1 = await message.ReadRawMessageAsync(false);
            Assert.Contains("HelloWorld", raw1);

            var raw2 = await message.ReadRawMessageAsync(true);
            Assert.DoesNotContain("HelloWorld", raw2);
        }

        [Fact]
        public async Task ReadRawMessageAsync_headers_Test()
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "/foo/bar");
            message.Headers.Clear();
            message.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

            var raw = await message.ReadRawMessageAsync();
            Assert.Contains("Accept-Charset", raw);

            var raw1 = await message.ReadRawMessageAsync(ignoreHeaders: new[] { "Accept-Charset" });
            Assert.DoesNotContain("utf-8", raw1);

            var raw2 = await message.ReadRawMessageAsync(ignoreHeaders: new string[] { });
            Assert.Contains("utf-8", raw2);
        }
    }
}
