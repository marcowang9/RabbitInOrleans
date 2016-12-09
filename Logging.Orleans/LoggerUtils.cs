using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Orleans
{
    public static class LoggerUtils
    {
        public static void LogAndThrownInvalidOp(string message, IOrleansLog logger)
        {
            var exception = new InvalidOperationException(message);
            if (logger != null)
            {
                logger.Error(exception);
            }
            throw exception;
        }
    }
}
