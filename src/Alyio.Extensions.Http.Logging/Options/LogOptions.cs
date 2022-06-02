using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Alyio.Extensions.Http.Logging;

/// <summary>
/// Provides programmatic configuration for http logging handler.
/// </summary>
public sealed class LogOptions
{
    /// <summary>
    /// Gets or sets a <see cref="LogLevel"/> value that whether the logger is enalble; The default is <see cref="LogLevel.Information" />.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets a <see cref="bool"/> value that indicates to ignore the request content. The default is true.
    /// </summary>
    public bool IgnoreRequestContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a <see cref="string"/> array to ignore the specified headers of <see cref="HttpRequestMessage.Headers"/>.
    /// </summary>
    public string[] IgnoreRequestHeaders { get; set; } = new string[] { };

    /// <summary>
    /// Gets or sets a <see cref="bool"/> value that indicates to ignore the response content. The default is true.
    /// </summary>
    public bool IgnoreResponseContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a <see cref="string"/> array to ignore the specified headers of <see cref="HttpResponseMessage.Headers"/>.
    /// </summary>
    public string[] IgnoreResponseHeaders { get; set; } = new string[] { };
}
