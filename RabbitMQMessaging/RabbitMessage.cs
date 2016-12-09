using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public class RabbitMessage
    {
        public RabbitMessage(IModel model)
        {
            BasicProperties = model.CreateBasicProperties();
            BasicProperties.Headers = new Dictionary<string, object>();
        }

        public IBasicProperties BasicProperties { get; set; }

        public byte[] Body { get; set; }

        public IDictionary<string, object> Properties
        {
            get { return BasicProperties.Headers; }
            set
            {
                foreach (var pair in value)
                {
                    BasicProperties.Headers.Add(pair.Key, pair.Value);
                }
            }
        }

        public TimeSpan TimeToLive
        {
            get { return new TimeSpan(Convert.ToInt64(BasicProperties.Expiration) * TimeSpan.TicksPerMillisecond); }
            set { BasicProperties.Expiration = Convert.ToInt64(value.TotalMilliseconds).ToString(); }
        }

        public String Type
        {
            get { return BasicProperties.Type; }
            set { BasicProperties.Type = value; }
        }
    }
}
