using System;
using System.IO;
using System.Runtime.CompilerServices;
using global::Orleans.Runtime;

namespace Logging.Orleans
{
    internal class OrleansLog : IOrleansLog
    {
        private Logger _logger;

        public void Initialize(Logger logger)
        {
            _logger = logger;
        }

        public void Information(string message, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage("{0}", filePath, memberName, lineNumber);

            _logger.Info(logCode ?? Constants.DefaultInfoLogId, logMessageFormat, message);
        }

        public void Information(string messageFormat, object[] logMessageParams, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage(messageFormat, filePath, memberName, lineNumber);
            _logger.Info(logCode ?? Constants.DefaultInfoLogId, logMessageFormat, logMessageParams);
        }

        public void Warning(string message, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage("{0}", filePath, memberName, lineNumber);
            _logger.Warn(logCode ?? Constants.DefaultWarningLogId, logMessageFormat, message);
        }

        public void Warning(string messageFormat, object[] logMessageParams, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage(messageFormat, filePath, memberName, lineNumber);
            _logger.Warn(logCode ?? Constants.DefaultWarningLogId, logMessageFormat, logMessageParams);
        }

        public void Error(string errorMessage, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage(errorMessage, filePath, memberName, lineNumber);

            _logger.Error(logCode ?? Constants.DefaultErrorLogId, logMessageFormat);
        }

        public void Error(string messageFormat, object[] logMessageParams, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage(messageFormat, filePath, memberName, lineNumber);
            var errorMessage = string.Format(logMessageFormat, logMessageParams);

            _logger.Error(logCode ?? Constants.DefaultErrorLogId, errorMessage);
        }

        public void Error(Exception exception, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            var logMessageFormat = FormatMessage(exception.Message, filePath, memberName, lineNumber);
            _logger.Error(logCode ?? Constants.DefaultErrorLogId, logMessageFormat, exception);
        }

        public void MemberEntry([CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            _logger.Info(GetMemberDetails(filePath, memberName, lineNumber) + " --> Entry");
        }

        public void MemberEntry(string uniqueId, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            _logger.Info("{0}:{1} - line #: {2} uniqueId: {3} --> Entry", Path.GetFileNameWithoutExtension(filePath), memberName, lineNumber, uniqueId);
        }

        public void MemberExit([CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            _logger.Info(GetMemberDetails(filePath, memberName, lineNumber) + " --> Exit");
        }

        public void MemberExit(string uniqueId, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            _logger.Info("{0}:{1} - line #: {2} uniqueId: {3} --> Exit", Path.GetFileNameWithoutExtension(filePath), memberName, lineNumber, uniqueId);
        }

        private static string GetMemberDetails(string filePath, string memberName, int lineNumber)
        {
            return string.Format("{0}:{1} - line #: {2}", Path.GetFileNameWithoutExtension(filePath), memberName, lineNumber);
        }

        private static string FormatMessage(string messageFormat, string filePath, string memberName, int lineNumber)
        {
            return string.Format("{0} -- {1}", GetMemberDetails(filePath, memberName, lineNumber), messageFormat);
        }
    }

    public class Constants
    {
        public const int DefaultInfoLogId = 1000;
        public const int DefaultWarningLogId = 1001;
        public const int DefaultErrorLogId = 1002;
    }
}
