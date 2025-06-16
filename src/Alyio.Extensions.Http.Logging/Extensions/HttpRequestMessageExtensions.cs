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
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var strBuilder = new StringBuilder(128);
        strBuilder.Append(CultureInfo.InvariantCulture, $"{request.Method} {request.RequestUri} HTTP/{request.Version}");
        strBuilder.Append(Environment.NewLine);

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            if (ignoreHeaders?.Contains(header.Key) == true) { continue; }
            strBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
            strBuilder.Append(Environment.NewLine);
        }

        if (request.Content != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
            {
                strBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
                strBuilder.Append(Environment.NewLine);
            }
            strBuilder.Append(Environment.NewLine);

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
                        strBuilder.Append(CultureInfo.InvariantCulture, $"--{boundary}");
                        strBuilder.Append(Environment.NewLine);
                        strBuilder.Append(CultureInfo.InvariantCulture, $"{HeaderNames.ContentDisposition}: {content.Headers.ContentDisposition}");
                        strBuilder.Append(Environment.NewLine);
                        if (content.Headers.ContentType != null)
                        {
                            strBuilder.Append(CultureInfo.InvariantCulture, $"{HeaderNames.ContentType}: {content.Headers.ContentType}");
                            strBuilder.Append(Environment.NewLine);
                        }
                        strBuilder.Append(Environment.NewLine);
                        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                        {
                            if (header.Key == HeaderNames.ContentDisposition || header.Key == HeaderNames.ContentType) { continue; }

                            strBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
                            strBuilder.Append(Environment.NewLine);
                        }
                        strBuilder.Append(Environment.NewLine);

                        if (!ignoreContent)
                        {
                            (string contentString, HttpContent newContent) = await content.ReadContentAsStringAsync(cancellationToken);
                            strBuilder.Append(contentString);
                            strBuilder.Append(Environment.NewLine);
                            duplicatedFormData.Add(newContent);
                        }
                    }
                }
                strBuilder.Append(CultureInfo.InvariantCulture, $"--{boundary}--");
                strBuilder.Append(Environment.NewLine);
                request.Content = duplicatedFormData;
            }
            else if (!ignoreContent)
            {
                (string contentString, HttpContent newContent) = await request.Content.ReadContentAsStringAsync(cancellationToken);
                request.Content = newContent;
                strBuilder.Append(contentString);
            }
        }

        return strBuilder.ToString();
    }
}
