using System.Runtime.CompilerServices;
using Blogsphere.Notification.Service.Models.Constants;

namespace Blogsphere.Notification.Service.Extensions
{
    public static class LoggerExtensions
    {
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
            return logger.ForContext(LoggerConstants.CorrelationId, correlationId);
        }
    }
}