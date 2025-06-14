// MIT License

using System.Globalization;
using System.Text;

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
    /// <returns>The raw http message of <see cref="HttpRequestMessage"/>.</returns>
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

        if (!ignoreContent && request.Content != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
            {
                strBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}");
                strBuilder.Append(Environment.NewLine);
            }
            strBuilder.Append(Environment.NewLine);

            Stream content = await request.Content.ReadAsStreamAsync(cancellationToken);

            if (content.CanSeek)
            {
                using var reader = new StreamReader(content, leaveOpen: true);
                strBuilder.Append(await reader.ReadToEndAsync(cancellationToken));
                content.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var memo = new MemoryStream();
                try
                {
                    await content.CopyToAsync(memo, cancellationToken);
                    memo.Seek(0, SeekOrigin.Begin);
                }
                finally
                {
                    await content.DisposeAsync();
                }

                using var reader = new StreamReader(memo, leaveOpen: true);
                strBuilder.Append(await reader.ReadToEndAsync(cancellationToken));
                memo.Seek(0, SeekOrigin.Begin);

                var newContent = new StreamContent(content);
                foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
                {
                    newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                request.Content = newContent;
            }
        }

        return strBuilder.ToString();
    }
}
