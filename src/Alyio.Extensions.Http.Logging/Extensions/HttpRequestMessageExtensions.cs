// MIT License

using System.Globalization;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace Alyio.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpRequestMessage"/>.
/// </summary>
public static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Gets HTTP response message with headers and body.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
    /// <param name="ignoreContent">A <see cref="bool"/> value that indicates to ignore the request content. The default is false.</param>
    /// <param name="ignoreHeaders">The specified <see cref="string"/> array to ignore the specified headers of <see cref="HttpRequestMessage.Headers"/>.</param>
    /// <param name="redactHeaders">The specified <see cref="string"/> array to redact the specified headers of <see cref="HttpRequestMessage.Headers"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The raw http message of <see cref="HttpRequestMessage"/> in the format:
    /// <code>
    /// METHOD URI HTTP/VERSION
    /// Header1: Value1
    /// Header2: Value2
    /// ...
    /// 
    /// [Content]
    /// </code>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static async Task<string> ReadRawMessageAsync(
        this HttpRequestMessage request,
        bool ignoreContent = false,
        string[]? ignoreHeaders = null,
        string[]? redactHeaders = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rawMessageBuilder = new StringBuilder(128);
        rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{request.Method} {request.RequestUri} HTTP/{request.Version}");
        rawMessageBuilder.Append(Environment.NewLine);

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            if (ignoreHeaders?.Contains(header.Key) == true) { continue; }
            if (redactHeaders?.Contains(header.Key) == true)
            {
                rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: ***");
            }
            else
            {
                rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
            }
            rawMessageBuilder.Append(Environment.NewLine);
        }

        if (request.Content != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
            {
                if (ignoreHeaders?.Contains(header.Key) == true) { continue; }
                if (redactHeaders?.Contains(header.Key) == true)
                {
                    rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: ***");
                }
                else
                {
                    rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
                }
                rawMessageBuilder.Append(Environment.NewLine);
            }
            rawMessageBuilder.Append(Environment.NewLine);

            if (request.Content is MultipartFormDataContent originalFormData)
            {
                var duplicatedFormData = new MultipartFormDataContent();
                foreach (KeyValuePair<string, IEnumerable<string>> header in originalFormData.Headers)
                {
                    duplicatedFormData.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                string? boundary = originalFormData.Headers.ContentType?.Parameters.FirstOrDefault(p => p.Name == "boundary")?.Value;
                foreach (HttpContent content in originalFormData)
                {
                    if (content.Headers.ContentDisposition != null)
                    {
                        rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"--{boundary}");
                        rawMessageBuilder.Append(Environment.NewLine);
                        rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{HeaderNames.ContentDisposition}: {content.Headers.ContentDisposition}");
                        rawMessageBuilder.Append(Environment.NewLine);
                        if (content.Headers.ContentType != null)
                        {
                            rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{HeaderNames.ContentType}: {content.Headers.ContentType}");
                            rawMessageBuilder.Append(Environment.NewLine);
                        }
                        rawMessageBuilder.Append(Environment.NewLine);
                        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                        {
                            if (header.Key == HeaderNames.ContentDisposition || header.Key == HeaderNames.ContentType) { continue; }

                            rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
                            rawMessageBuilder.Append(Environment.NewLine);
                        }
                        rawMessageBuilder.Append(Environment.NewLine);

                        if (!ignoreContent)
                        {
                            (string contentString, HttpContent newContent) = await content.ReadRawMessageAsync(cancellationToken);
                            rawMessageBuilder.Append(contentString);
                            rawMessageBuilder.Append(Environment.NewLine);
                            duplicatedFormData.Add(newContent);
                        }
                    }
                }
                rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"--{boundary}--");
                rawMessageBuilder.Append(Environment.NewLine);
                request.Content = duplicatedFormData;
            }
            else if (!ignoreContent)
            {
                (string contentString, HttpContent newContent) = await request.Content.ReadRawMessageAsync(cancellationToken);
                request.Content = newContent;
                rawMessageBuilder.Append(contentString);
            }
        }

        return rawMessageBuilder.ToString();
    }
}
