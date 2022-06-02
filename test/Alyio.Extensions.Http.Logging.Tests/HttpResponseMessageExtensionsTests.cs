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
            var message = new HttpResponseMessage();
            message.Headers.Clear();
            message.Headers.Location = new Uri("http://localhost/foo/bar");

            var raw = await message.ReadRawMessageAsync();
            Assert.Contains("localhost", raw);

            var raw1 = await message.ReadRawMessageAsync(ignoreHeaders: new string[] { });
            Assert.Contains("localhost", raw1);

            var raw2 = await message.ReadRawMessageAsync(ignoreHeaders: new[] { "Location" });
            Assert.DoesNotContain("localhost", raw2);
        }
    }
}
