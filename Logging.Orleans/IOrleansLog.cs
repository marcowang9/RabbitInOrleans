using System;
using System.Runtime.CompilerServices;
using global::Orleans.Runtime;

namespace Logging.Orleans
{
    public interface IOrleansLog
    {
        void Initialize(Logger logger);

        void Information(string message, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "");

        void Information(string messageFormat, object[] logMessageParams, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "");

        void Warning(string message, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");

        void Warning(string messageFormat, object[] logMessageParams, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "");

        void Error(string errorMessage, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");

        void Error(Exception exception, int? logCode = null, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");

        void Error(string messageFormat, object[] logMessageParams, int? logCode = null,
            [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "");

        void MemberEntry([CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");

        void MemberEntry(string uniqueId, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");

        void MemberExit([CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");

        void MemberExit(string uniqueId, [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "");
    }
}
