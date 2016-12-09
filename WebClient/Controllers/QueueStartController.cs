using Common;
using RabbitMQ.Client;
using RabbitMQMessaging;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebClient.Controllers
{
    public class QueueStartController : ApiController
    {
        IFluentQueueSender _sender;

        [HttpPost]
        public async Task PostAsync(string deviceID)
        {
            CreateQueue(deviceID);

            var message = new RabbitSendMessage("StartQueue", new MessageDto() { Message = deviceID, MessageDirection = MessageDirection.ToStart})
            {
                Header = new MessageHeader() { ContentType = MimeTypes.ApplicationJson }
            };
            using (_sender = new RabbitMqQueueSender(ConnectionConst.AmqpConnection, ConnectionConst.Exchange, ConnectionConst.MessageQueue))
            {
                await _sender.Send(message);
            }
        }

        private void CreateQueue(string deviceId)
        {
            var connectionFactory = new ConnectionFactory() { Uri = ConnectionConst.AmqpConnection };
            string queueName = ConnectionConst.CommandQueuePrefix + deviceId;

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: ConnectionConst.CommandExchange,
                                                            type: "topic",
                                                           durable: true,
                                                           autoDelete: false,
                                                           arguments: null);

                channel.QueueDeclare(queue: queueName,
                                                      durable: true,
                                                      exclusive: false,
                                                      autoDelete: false,
                                                      arguments: null);

                channel.QueueBind(queue: queueName,
                                  exchange: ConnectionConst.CommandExchange,
                                  routingKey: deviceId);
            }
        }
    }
}