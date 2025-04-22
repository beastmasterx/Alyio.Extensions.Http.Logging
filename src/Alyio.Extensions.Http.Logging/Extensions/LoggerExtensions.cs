// MIT License

using Microsoft.Extensions.Logging;

namespace Alyio.Extensions.Http.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Message = "Request-Queue: {RequestCount}")]
        public static partial void RequestQueue(this ILogger logger, LogLevel level, int requestCount);


        [LoggerMessage(Message = "Request-Message: {NewLine}{RequestRawMessage}")]
        public static partial void RequestMessage(this ILogger logger, LogLevel level, string newLine, string requestRawMessage);


        [LoggerMessage(Level = LogLevel.Error, Message = "Request-Error: {Message}, elapsed: {ElapsedMilliseconds}ms{NewLine}{RequestRawMessage}")]
        public static partial void RequestError(
            this ILogger logger,
            Exception exception,
            string message,
            long elapsedMilliseconds,
            string newLine,
            string requestRawMessage);

        [LoggerMessage(Message = "Response-Message: {ElapsedMilliseconds}ms{NewLine}{ResponseRawMessage}")]
        public static partial void ResponseMessage(
            this ILogger logger,
            LogLevel level,
            long elapsedMilliseconds,
            string newLine,
            string responseRawMessage);
    }
}
