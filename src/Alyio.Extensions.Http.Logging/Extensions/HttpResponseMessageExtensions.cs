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
    /// <returns>The raw http message of <see cref="HttpResponseMessage"/>.</returns>
    public static async Task<string> ReadRawMessageAsync(this HttpResponseMessage response, bool ignoreContent = false, params string[] ignoreHeaders)
    {
        StringBuilder strBuilder = new(128);
        strBuilder.Append(CultureInfo.InvariantCulture, $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}\r\n");
        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
        {
            if (ignoreHeaders.Contains(header.Key)) { continue; }
            strBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}\r\n");
        }
        if (!ignoreContent && response.Content != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
            {
                strBuilder.Append(CultureInfo.InvariantCulture, $"{header.Key}: {string.Join(",", header.Value)}\r\n");
            }
            strBuilder.Append("\r\n");
            Stream content = await response.Content.ReadAsStreamAsync();
            if (content.CanSeek)
            {
                StreamReader reader = new(content);
                strBuilder.Append(await reader.ReadToEndAsync());
                content.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var memo = new MemoryStream();
                await content.CopyToAsync(memo);
                memo.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new(memo);
                strBuilder.Append(await reader.ReadToEndAsync());
                memo.Seek(0, SeekOrigin.Begin);
                System.Net.Http.Headers.HttpContentHeaders contentHeaders = response.Content.Headers;
                response.Content = new StreamContent(memo);
                response.Content.Headers.Clear();
                foreach (string item in contentHeaders.Allow)
                {
                    response.Content.Headers.Allow.Add(item);
                }
                foreach (string item in contentHeaders.ContentEncoding)
                {
                    response.Content.Headers.ContentEncoding.Add(item);
                }
                foreach (string item in contentHeaders.ContentLanguage)
                {
                    response.Content.Headers.ContentLanguage.Add(item);
                }
                response.Content.Headers.ContentDisposition = contentHeaders.ContentDisposition;
                response.Content.Headers.ContentLength = contentHeaders.ContentLength;
                response.Content.Headers.ContentLocation = contentHeaders.ContentLocation;
                response.Content.Headers.ContentMD5 = contentHeaders.ContentMD5;
                response.Content.Headers.ContentRange = contentHeaders.ContentRange;
                response.Content.Headers.ContentType = contentHeaders.ContentType;
                response.Content.Headers.Expires = contentHeaders.Expires;
                response.Content.Headers.LastModified = contentHeaders.LastModified;
            }
        }
        return strBuilder.ToString();
    }
}
