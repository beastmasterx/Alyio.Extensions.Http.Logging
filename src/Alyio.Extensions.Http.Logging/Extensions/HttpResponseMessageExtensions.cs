// MIT License

using System.Globalization;
using System.Text;

namespace Alyio.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpResponseMessage"/>.
/// </summary>
public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Gets HTTP response message with headers and body.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
    /// <param name="ignoreContent">A <see cref="bool"/> value that indicates to ignore the response content. The default is false.</param>
    /// <param name="ignoreHeaders">The specified <see cref="string"/> array to ignore the specified headers of <see cref="HttpResponseMessage.Headers"/>.</param>
    /// <param name="redactHeaders">The specified <see cref="string"/> array to redact the specified headers of <see cref="HttpResponseMessage.Headers"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The raw http message of <see cref="HttpResponseMessage"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static async Task<string> ReadRawMessageAsync(
        this HttpResponseMessage response,
        bool ignoreContent = false,
        string[]? ignoreHeaders = null,
        string[]? redactHeaders = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        var rawMessageBuilder = new StringBuilder(128);
        rawMessageBuilder.Append(CultureInfo.InvariantCulture, $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
        rawMessageBuilder.Append(Environment.NewLine);

        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
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

        if (response.Content != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
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

            if (!ignoreContent)
            {
                (string text, HttpContent newContent) = await response.Content.ReadRawMessageAsync(cancellationToken);
                rawMessageBuilder.Append(text);
                response.Content = newContent;
            }
        }

        return rawMessageBuilder.ToString();
    }
}
