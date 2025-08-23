// MIT License

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alyio.Extensions.Http.Logging
{
    /// <summary>
    /// The message handler used by <see cref="HttpClient"/> for logging raw HTTP request and response messages.
    /// </summary>
    public sealed class HttpRawMessageLoggingHandler : DelegatingHandler
    {
        private static readonly string s_messageSeparator = Environment.NewLine + Environment.NewLine;

        private int _activeRequestCount;
        private readonly ILogger _logger;
        private readonly HttpRawMessageLoggingOptions _loggingOptions;

        /// <summary>
        /// Creates an instance of a <see cref="HttpRawMessageLoggingHandler"/> class.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="logOptions">The <see cref="HttpRawMessageLoggingOptions"/></param>
        public HttpRawMessageLoggingHandler(ILoggerFactory loggerFactory, IOptions<HttpRawMessageLoggingOptions> logOptions)
        {
            _loggingOptions = logOptions.Value;
            _logger = loggerFactory.CreateLogger(_loggingOptions.CategoryName ?? GetType().FullName!);
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpResponseMessage"/> based on the information provided in the <see cref="HttpRequestMessage"/> as an operation that will not block.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>Returns <see cref="Task{TResult}"/>. The task object representing the asynchronous operation.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(_loggingOptions.Level))
            {
                return SendCoreAsync(request, cancellationToken);
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }

        /// <summary>
        /// Core implementation of sending the HTTP request and logging both request and response messages.
        /// </summary>
        private async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.RequestQueue(_loggingOptions.Level, Interlocked.Increment(ref _activeRequestCount));

            var watch = Stopwatch.StartNew();
            string requestRawMessage = await request.ReadRawMessageAsync(
                _loggingOptions.IgnoreRequestContent, _loggingOptions.IgnoreRequestHeaders, _loggingOptions.RedactRequestHeaders, cancellationToken);
            HttpResponseMessage? responseMessage = null;

            try
            {
                await LogRequestMessageAsync(requestRawMessage);
                responseMessage = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                await LogRequestErrorAsync(ex, watch.ElapsedMilliseconds, requestRawMessage);
                throw;
            }
            finally
            {
                Interlocked.Decrement(ref _activeRequestCount);
            }

            string responseRawMessage = await responseMessage.ReadRawMessageAsync(
                 _loggingOptions.IgnoreResponseContent, _loggingOptions.IgnoreResponseHeaders, _loggingOptions.RedactResponseHeaders, cancellationToken);
            await LogResponseMessageAsync(responseRawMessage, watch.ElapsedMilliseconds);

            return responseMessage;
        }

        /// <summary>
        /// Logs the raw HTTP request message.
        /// </summary>
        private ValueTask LogRequestMessageAsync(string requestRawMessage)
        {
            _logger.RequestMessage(_loggingOptions.Level, s_messageSeparator, requestRawMessage);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Logs any error that occurs during the HTTP request.
        /// </summary>
        private ValueTask LogRequestErrorAsync(HttpRequestException ex, long elapsedMilliseconds, string requestRawMessage)
        {
            _logger.RequestError(ex, ex.Message, elapsedMilliseconds, s_messageSeparator, requestRawMessage);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Logs the raw HTTP response message.
        /// </summary>
        private ValueTask LogResponseMessageAsync(string responseRawMessage, long elapsedMilliseconds)
        {
            _logger.ResponseMessage(_loggingOptions.Level, elapsedMilliseconds, s_messageSeparator, responseRawMessage);
            return ValueTask.CompletedTask;
        }
    }
}

