using System.Linq;
using System.Runtime.CompilerServices;
using Blogsphere.Notification.Service.Models.Constants;

namespace Blogsphere.Notification.Service.Extensions
{
    public static class LoggerExtensions
    {
        private const int MaxLoggedMessageLength = 200;

        public static ILogger Here(this ILogger logger,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string caller = ""
        )
        {
            var callerType = Path.GetFileNameWithoutExtension(caller);

            return logger.ForContext(LoggerConstants.MemberName, memberName)
                .ForContext(LoggerConstants.CallerType, callerType);
        }

        public static void MethodEntered(this ILogger logger)
        {
            logger.Debug(LoggerConstants.MethodEntered);
        }

        public static void MethodExited(this ILogger logger)
        {
            logger.Debug(LoggerConstants.MethodExited);
        }

        public static ILogger WithCorrelationId(this ILogger logger, string correlationId)
        {
            return string.IsNullOrEmpty(correlationId)
                ? logger
                : logger.ForContext(LoggerConstants.CorrelationId, correlationId);
        }

        /// <summary>
        /// Logs an exception safely without exposing stack traces, connection strings, or other sensitive data.
        /// </summary>
        public static void LogErrorSafely(this ILogger logger, Exception ex, string correlationId,
            string messageTemplate, params object[] args)
        {
            LogExceptionSafely(logger, ex, correlationId, (l, t, a) => l.Error(t, a), messageTemplate, args);
        }

        /// <summary>
        /// Logs an exception as warning safely without exposing stack traces or sensitive data.
        /// </summary>
        public static void LogWarningSafely(this ILogger logger, Exception ex, string correlationId,
            string messageTemplate, params object[] args)
        {
            LogExceptionSafely(logger, ex, correlationId, (l, t, a) => l.Warning(t, a), messageTemplate, args);
        }

        private static void LogExceptionSafely(ILogger logger, Exception ex, string correlationId,
            Action<ILogger, string, object[]> logAction, string messageTemplate, object[] args)
        {
            var safeMessage = Truncate(ex.Message, MaxLoggedMessageLength);
            var exceptionType = ex.GetType().Name;

            var enriched = logger
                .WithCorrelationId(correlationId)
                .ForContext("ExceptionType", exceptionType)
                .ForContext("SafeMessage", safeMessage);

            var allArgs = args.Concat(new object[] { exceptionType, safeMessage }).ToArray();
            var fullTemplate = messageTemplate + " [ExceptionType={ExceptionType}, SafeMessage={SafeMessage}]";
            logAction(enriched, fullTemplate, allArgs);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }
    }
}