using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public class AbandonException : Exception
    {
        public new Exception InnerException { get; }
        public new string Message { get; }

        public AbandonException()
        {
        }

        public AbandonException(string message)
        {
            Message = message;
        }

        public AbandonException(string message, Exception innerException)
        {
            Message = message;
            InnerException = innerException;
        }
    }

    public class RabbitMQConnectionException : Exception
    {
        public new Exception InnerException { get; }
        public new string Message { get; }

        public RabbitMQConnectionException()
        {
        }

        public RabbitMQConnectionException(string message)
        {
            Message = message;
        }

        public RabbitMQConnectionException(string message, Exception innerException)
        {
            Message = message;
            InnerException = innerException;
        }
    }

    public class RabbitMQException : Exception
    {
        public new Exception InnerException { get; }
        public new string Message { get; }

        public RabbitMQException()
        {
        }

        public RabbitMQException(string message)
        {
            Message = message;
        }

        public RabbitMQException(string message, Exception innerException)
        {
            Message = message;
            InnerException = innerException;
        }
    }
}
