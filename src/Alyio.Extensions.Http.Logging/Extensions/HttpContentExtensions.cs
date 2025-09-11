// MIT License

using Alyio.Extensions.Http.Logging;

namespace Alyio.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="HttpContent"/>.
    /// </summary>
    public static class HttpContentExtensions
    {
        private const int DefaultBufferSize = 81920; // 80KB buffer size

        /// <summary>
        /// Reads the content of the <see cref="HttpContent"/> as a string.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> to read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns>A tuple containing:
        /// <list type="bullet">
        /// <item><description>The string representation of the content</description></item>
        /// <item><description>The original or duplicated <see cref="HttpContent"/> that can be read again</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
        public static async Task<(string message, HttpContent content)> ReadRawMessageAsync(this HttpContent content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (MimeTypeChecker.IsTextBased(content.Headers.ContentType?.MediaType) is false)
            {
                return ($"[{content.Headers.ContentType?.MediaType ?? "unknown"}]", content);
            }

            string text = string.Empty;
            Stream contentStream = await content.ReadAsStreamAsync(cancellationToken);

            if (contentStream.CanSeek)
            {
                using var reader = new StreamReader(contentStream, leaveOpen: true);
#if !NET6_0
                text = await reader.ReadToEndAsync(cancellationToken);
#else
                text = await reader.ReadToEndAsync();
#endif
                contentStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var memo = new MemoryStream();
                try
                {
                    await contentStream.CopyToAsync(memo, DefaultBufferSize, cancellationToken);
                    memo.Seek(0, SeekOrigin.Begin);
                }
                finally
                {
                    await contentStream.DisposeAsync();
                }

                using var reader = new StreamReader(memo, leaveOpen: true);
#if !NET6_0
                text = await reader.ReadToEndAsync(cancellationToken);
#else
                text = await reader.ReadToEndAsync();
#endif
                memo.Seek(0, SeekOrigin.Begin);

                var newContent = new StreamContent(memo);
                foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                {
                    newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                content = newContent;
            }

            return (text, content);
        }
    }
}
