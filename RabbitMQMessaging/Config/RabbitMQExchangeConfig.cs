using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public class TestRabbitMQExchangeConfig : IRabbitMQExchangeConfig
    {
        public string ExchangeName { get { return "marcotest"; } }

        public string ExchangeType { get { return "direct"; } }

        public bool Durable { get { return true; } }

        public bool AutoDelete { get { return false; } }

        public IDictionary<string, object> Arguments { get; set; }
    }
}
