// MIT License

using Microsoft.Extensions.Logging;

namespace Alyio.Extensions.Http.Logging;

/// <summary>
/// Provides programmatic configuration for HTTP message logging.
/// </summary>
public sealed class HttpMessageLoggingOptions
{
    /// <summary>
    /// Gets or sets the category name for the logger.
    /// </summary>
    /// <remarks>
    /// The default is the fully qualified name of the <see cref="HttpRawMessageLoggingHandler"/>, 
    /// including the namespace but not the assembly.
    /// </remarks>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level for HTTP message logging.
    /// </summary>
    /// <remarks>
    /// Messages below this level will not be logged.
    /// The default is <see cref="LogLevel.Information" />.
    /// </remarks>
    public LogLevel Level { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets whether to ignore the request content in logs.
    /// </summary>
    /// <remarks>
    /// When true, the request body will not be included in the logs.
    /// The default is true.
    /// </remarks>
    public bool IgnoreRequestContent { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of request header names to ignore in logs.
    /// </summary>
    /// <remarks>
    /// Headers with names in this collection will not be included in the logs.
    /// The default is an empty array.
    /// </remarks>
    public string[] IgnoreRequestHeaders { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to ignore the response content in logs.
    /// </summary>
    /// <remarks>
    /// When true, the response body will not be included in the logs.
    /// The default is true.
    /// </remarks>
    public bool IgnoreResponseContent { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of response header names to ignore in logs.
    /// </summary>
    /// <remarks>
    /// Headers with names in this collection will not be included in the logs.
    /// The default is an empty array.
    /// </remarks>
    public string[] IgnoreResponseHeaders { get; set; } = [];
}
