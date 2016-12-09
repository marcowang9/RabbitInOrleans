using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Runtime.Serialization;
using Common;

namespace RabbitMQMessaging
{
    [Serializable]
    public class RabbitReceivedMessage : IReceivedMessage, ISerializable
    {
        private readonly BasicDeliverEventArgs _eventArgs;

        private readonly IModel _model;

        public IBasicProperties BasicProperties { get; set; }

        public byte[] Body { get; set; }

        public MessageHeader Header { get; private set; }

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

        public ulong DeliveryTag { get; set; }

        public MessageDirection Direction  { get; set; }

        public RabbitReceivedMessage(
            IModel model, BasicDeliverEventArgs eventArgs)
        {
            _model = model;
            _eventArgs = eventArgs;
            BasicProperties = model.CreateBasicProperties();
            BasicProperties.Headers = new Dictionary<string, object>();

            Header = new MessageHeader()
            {
                To = eventArgs.ConsumerTag,
                MessageId = eventArgs.BasicProperties.MessageId,
                ContentType = eventArgs.BasicProperties.ContentType,
                CorrelationId = eventArgs.BasicProperties.CorrelationId,
                ReplyTo = eventArgs.BasicProperties.ReplyTo,
            };
        }

        protected RabbitReceivedMessage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            Header = (MessageHeader)info.GetValue("Header", typeof(MessageHeader));
            Body = (byte[])info.GetValue("Body", typeof(byte[]));
        }

        public async Task AbandonAsync()
        {
            await Task.Run(() => _model.BasicNack(_eventArgs.DeliveryTag, false, true));
        }

        public async Task CompleteAsync()
        {
            await Task.Run(()=>_model.BasicAck(_eventArgs.DeliveryTag, false));
        }

        public byte[] GetBody()
        {
            return Body;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            info.AddValue("Header", Header);
            info.AddValue("Body", Body);
        }
    }
}
