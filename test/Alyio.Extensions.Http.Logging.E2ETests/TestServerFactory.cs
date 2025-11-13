// MIT License

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.Extensions.Http.Logging.E2ETests;

internal static class TestServerFactory
{
    internal const string E2E_TEST_URL_GET = "/get";
    internal const string E2E_TEST_URL_POST = "/post";
    internal const string E2E_TEST_URL_HEADERS = "/headers";
    internal const string E2E_TEST_URL_STATUS_404 = "/status/404";
    internal const string E2E_TEST_URL_IMAGE_PNG = "/image/png";
    internal const string E2E_TEST_URL_MULTIPART_FORM_DATA = "/multipart/form-data";

    // a 1x1 PNG image (black pixel)
    internal static byte[] ImageBytes => new byte[]
    {
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
        0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
        0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
        0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
        0x42, 0x60, 0x82
    };

    internal static async Task<TestServer> CreateTestServerAsync(Action<HttpRawMessageLoggingOptions>? configureOptions = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        var httpClientBuilder = builder.Services.AddHttpClient("Test");
        if (configureOptions != null)
        {
            httpClientBuilder.AddHttpRawMessageLogging(configureOptions);
        }
        else
        {
            httpClientBuilder.AddHttpRawMessageLogging();
        }

        var app = builder.Build();

        app.MapGet(E2E_TEST_URL_GET, (HttpRequest request) => Results.Ok(new
        {
            message = "This is a GET response",
            requestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value),
        }));
        app.MapPost(E2E_TEST_URL_POST, async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            return Results.Ok(new
            {
                message = "This is a POST response",
                requestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value),
                requestBody = body,
            });
        });
        app.MapGet(E2E_TEST_URL_HEADERS, (HttpRequest request) => Results.Ok(new
        {
            message = "This is a HEADERS response",
            requestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value)
        }));
        app.MapGet(E2E_TEST_URL_STATUS_404, (HttpRequest request) => Results.NotFound(new
        {
            message = "This is a 404 response",
            requestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value),
        }));
        app.MapGet(E2E_TEST_URL_IMAGE_PNG, () => Results.File(ImageBytes, "image/png"));
        app.MapPost(E2E_TEST_URL_IMAGE_PNG, async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            return Results.Ok(new
            {
                message = "This is a POST response",
                requestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value),
                requestBody = body,
            });
        });
        app.MapPost(E2E_TEST_URL_MULTIPART_FORM_DATA, async (HttpRequest request) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Expected multipart/form-data");
            }

            var form = await request.ReadFormAsync();

            var fields = form.Keys
                .Where(k => !form.Files.Any(f => f.Name == k))
                .ToDictionary(k => k, v => form[v].ToString());

            var files = form.Files.Select(f => new
            {
                name = f.Name,
                fileName = f.FileName,
                contentType = f.ContentType,
                length = f.Length
            }).ToList();

            return Results.Ok(new
            {
                message = "This is a multipart form-data POST response",
                requestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString()),
                formFields = fields,
                formFiles = files
            });
        });

        await app.StartAsync();

        var server = app.GetTestServer();

        return server;
    }
}
