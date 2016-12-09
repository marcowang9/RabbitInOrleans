using Microsoft.WindowsAzure;

namespace Common
{
    static public class ConnectionConst
    {
        public const string amqpConnection = "amqp://@localhost/";

        public const string exchange = "";

        public const string commandExchange = "CommandExchange";

        public const string commandQueue = "CommandQueue";

        public const string messageQueue = "MessageQueue";

        public const string dataQueue = "DataQueue";

        static bool _isInAzure;

        static ConnectionConst()
        {
            _isInAzure = AzureEnvironment.IsInAzure;
        }

        public static string AmqpConnection
        {
            get
            {
                return _isInAzure ? CloudConfigurationManager.GetSetting("RabbitMQConnectionString") : amqpConnection;
                //return amqpConnection;
            }
        }

        public static string Exchange
        {
            get
            {
                //return _isInAzure ? CloudConfigurationManager.GetSetting("Exchange") : exchange;
                return exchange;
            }
        }

        public static string CommandExchange
        {
             get
            {
                return commandExchange;
            }
        }

        public static string CommandQueuePrefix
        {
            get { return "commandQ - "; }
        } 

        public static string CommandQueue
        {
            get
            {
                return _isInAzure ? CloudConfigurationManager.GetSetting("CommandQueueName") : commandQueue;
            }
        }

        public static string MessageQueue
        {
            get
            {
                return _isInAzure ? CloudConfigurationManager.GetSetting("MessageQueueName") : messageQueue;
            }
        }

        public static string DataQueue
        {
            get
            {
                return _isInAzure ? CloudConfigurationManager.GetSetting("DataQueueName") : dataQueue;
            }
        }
    }
}
