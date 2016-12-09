using System.Collections.Generic;

namespace RabbitMQMessaging
{
    public interface IRabbitMQExchangeConfig
    {
        IDictionary<string, object> Arguments { get; }
        bool AutoDelete { get; }
        bool Durable { get; }
        string ExchangeName { get; }
        string ExchangeType { get; }
    }
}