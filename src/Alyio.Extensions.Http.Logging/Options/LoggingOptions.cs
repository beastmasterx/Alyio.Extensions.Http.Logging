using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Alyio.Extensions.Http.Logging;

/// <summary>
/// Provides programmatic configuration for http logging handler.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>
    /// Gets or sets the category name for the logging handler.
    /// </summary>
    /// <remarks>The default is the the fully qualified name of the <see cref="LoggingHandler"/>, including the namespace of the <see cref="LoggingHandler"/> but not the assembly.</remarks>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="Level"/> value that whether the logger is enalble; The default is <see cref="LogLevel.Information" />.
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets a <see cref="bool"/> value that indicates to ignore the request content. The default is true.
    /// </summary>
    public bool RequestContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a <see cref="string"/> array to ignore the specified headers of <see cref="HttpRequestMessage.Headers"/>.
    /// </summary>
    public string[] RequestHeaders { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a <see cref="bool"/> value that indicates to ignore the response content. The default is true.
    /// </summary>
    public bool ResponseContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a <see cref="string"/> array to ignore the specified headers of <see cref="HttpResponseMessage.Headers"/>.
    /// </summary>
    public string[] ResponseHeaders { get; set; } = Array.Empty<string>();
}
