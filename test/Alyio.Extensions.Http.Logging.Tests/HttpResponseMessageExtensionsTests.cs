// MIT License

using System.Text;

namespace Alyio.Extensions.Http.Logging.Tests
{
    public class HttpResponseMessageExtensionsTests
    {
        [Fact]
        public async Task ReadRawMessageAsync_body_Test()
        {
            var message = new HttpResponseMessage
            {
                Content = new StringContent("HelloWorld", Encoding.UTF8, "application/json")
            };

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains("HelloWorld", raw);

            string raw1 = await message.ReadRawMessageAsync(false);
            Assert.Contains("HelloWorld", raw1);

            string raw2 = await message.ReadRawMessageAsync(true);
            Assert.DoesNotContain("HelloWorld", raw2);
        }

        [Fact]
        public async Task ReadRawMessageAsync_headers_Test()
        {
            var message = new HttpResponseMessage();
            message.Headers.Clear();
            message.Headers.Location = new Uri("http://localhost/foo/bar");

            string raw = await message.ReadRawMessageAsync();
            Assert.Contains("localhost", raw);

            string raw1 = await message.ReadRawMessageAsync(ignoreHeaders: []);
            Assert.Contains("localhost", raw1);

            string raw2 = await message.ReadRawMessageAsync(ignoreHeaders: ["Location"]);
            Assert.DoesNotContain("localhost", raw2);
        }
    }
}
