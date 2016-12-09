using System;
using global::Orleans.Runtime;

namespace Logging.Orleans
{
    public sealed class OrleansLoggerSingleton
    {
        private static readonly Lazy<IOrleansLog> _loggerInstance = new Lazy<IOrleansLog>(() => new OrleansLog());

        private OrleansLoggerSingleton()
        {

        }

        public static IOrleansLog WithLogger(Logger logger)
        {
            _loggerInstance.Value.Initialize(logger);
            return _loggerInstance.Value;
        }

        public static IOrleansLog Logger
        {
            get { return _loggerInstance.Value; }
        }
    }
}
