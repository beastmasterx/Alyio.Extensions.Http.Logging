// MIT License

using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddHttpClient<IHttpBinService, HttpBinService>(client =>
{
    client.BaseAddress = new Uri("https://httpbin.org");
})
.AddHttpRawMessageLogging(options =>
{
    options.IgnoreRequestContent = false;
    options.IgnoreResponseContent = false;
});

services.AddLogging(builder =>
{
    builder.AddDebug();
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning)
    .AddFilter("System.Net.Http.HttpClient", LogLevel.Information);
});

var httpbin = services.BuildServiceProvider().GetRequiredService<IHttpBinService>();

Console.WriteLine("\nCalling https://httpbin.org/user-agent ...");
await httpbin.GetUserAgentAsync();

Console.WriteLine("\nCalling https://httpbin.org/image/png ...");
await httpbin.GetImagePNGAsync();

Console.WriteLine("\nPosting 1x1 PNG image to https://httpbin.org/anything ...");
await httpbin.PostImagePNGAsync();

Console.WriteLine("\nPosting a short fake MP3 file to https://httpbin.org/anything ...");
await httpbin.PostAudioMpegAsync();

internal interface IHttpBinService
{
    Task<HttpResponseMessage> GetUserAgentAsync();

    Task<HttpResponseMessage> GetImagePNGAsync();

    Task<HttpResponseMessage> PostImagePNGAsync();

    Task<HttpResponseMessage> PostAudioMpegAsync();
}

sealed class HttpBinService : IHttpBinService
{
    private readonly HttpClient _client;

    public HttpBinService(HttpClient client)
    {
        _client = client;
    }

    public Task<HttpResponseMessage> GetUserAgentAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/user-agent");
        request.Headers.UserAgent.ParseAdd("Alyio.Extensions.Http.Logging.Samples/1.0");
        return _client.SendAsync(request);
    }

    public Task<HttpResponseMessage> GetImagePNGAsync()
    {
        return _client.GetAsync("/image/png");
    }

    public Task<HttpResponseMessage> PostImagePNGAsync()
    {
        // 1x1 PNG image (black pixel)
        var imageBytes = new byte[] {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
            0xDE, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D,
            0xB1, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
            0x44, 0xAE, 0x42, 0x60, 0x82
        };
        var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        var request = new HttpRequestMessage(HttpMethod.Post, "/anything")
        {
            Content = content
        };

        return _client.SendAsync(request);
    }

    public Task<HttpResponseMessage> PostAudioMpegAsync()
    {
        // a short, fake MP3 header (not a real audio file)
        var audioBytes = new byte[]
        {
            0x49, 0x44, 0x33, // "ID3" header for MP3
            0x03, 0x00, 0x00, 0x00, 0x00, 0x21, 0x76 // Some random bytes
        };
        var content = new ByteArrayContent(audioBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/anything")
        {
            Content = content
        };

        return _client.SendAsync(request);
    }
}
