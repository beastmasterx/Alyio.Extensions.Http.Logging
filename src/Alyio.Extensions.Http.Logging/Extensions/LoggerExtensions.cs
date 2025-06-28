// MIT License

using Microsoft.Extensions.Logging;

namespace Alyio.Extensions.Http.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(EventId = 1001, Message = "Request-Queue: {RequestCount}")]
        public static partial void RequestQueue(this ILogger logger, LogLevel level, int requestCount);


        [LoggerMessage(EventId = 1002, Message = "Request-Message: {NewLine}{RequestRawMessage}")]
        public static partial void RequestMessage(this ILogger logger, LogLevel level, string newLine, string requestRawMessage);


        [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "Request-Error: {Message}, elapsed: {ElapsedMilliseconds}ms{NewLine}{RequestRawMessage}")]
        public static partial void RequestError(
            this ILogger logger,
            Exception exception,
            string message,
            long elapsedMilliseconds,
            string newLine,
            string requestRawMessage);

        [LoggerMessage(EventId = 1004, Message = "Response-Message: {ElapsedMilliseconds}ms{NewLine}{ResponseRawMessage}")]
        public static partial void ResponseMessage(
            this ILogger logger,
            LogLevel level,
            long elapsedMilliseconds,
            string newLine,
            string responseRawMessage);
    }
}
