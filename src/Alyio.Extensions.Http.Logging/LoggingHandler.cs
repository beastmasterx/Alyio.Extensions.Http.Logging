// MIT License

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alyio.Extensions.Http.Logging
{
    /// <summary>
    /// The message handler used by <see cref="HttpClient"/> for logging http request and response message.
    /// </summary>
    public sealed class LoggingHandler : DelegatingHandler
    {
        private static readonly string s_doubleNewLine = Environment.NewLine + Environment.NewLine;

        private int _requestCount;
        private readonly ILogger _logger;
        private readonly LoggingOptions _logO;

        /// <summary>
        /// Creates an instance of a <see cref="LoggingHandler"/> class.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="logOptions">The <see cref="LoggingOptions"/></param>
        public LoggingHandler(ILoggerFactory loggerFactory, IOptions<LoggingOptions> logOptions)
        {
            _logO = logOptions.Value;
            _logger = loggerFactory.CreateLogger(_logO.CategoryName ?? GetType().FullName!);
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpResponseMessage"/> based on the information provided in the <see cref="HttpRequestMessage"/> as an operation that will not block.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>Returns <see cref="Task{TResult}"/>. The task object representing the asynchronous operation.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(_logO.Level))
            {
                return SendCoreAsync(request, cancellationToken);
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }

        private async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.RequestQueue(_logO.Level, Interlocked.Increment(ref _requestCount));

            Stopwatch watch = Stopwatch.StartNew();

            string requestRawMessage = await request.ReadRawMessageAsync(_logO.RequestContent, _logO.RequestHeaders);

            HttpResponseMessage? responseMessage = null;

            try
            {
                _logger.RequestMessage(_logO.Level, s_doubleNewLine, requestRawMessage);

                responseMessage = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.RequestError(ex, ex.Message, watch.ElapsedMilliseconds, s_doubleNewLine, requestRawMessage);

                throw;
            }
            finally
            {
                Interlocked.Decrement(ref _requestCount);
            }

            string responseRawMessage = await responseMessage.ReadRawMessageAsync(_logO.ResponseContent, _logO.ResponseHeaders);
            _logger.ResponseMessage(_logO.Level, watch.ElapsedMilliseconds, s_doubleNewLine, responseRawMessage);

            return await Task.FromResult(responseMessage);
        }
    }
}

