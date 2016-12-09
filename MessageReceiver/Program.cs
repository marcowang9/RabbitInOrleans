using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MessageReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            //var queueListner = new RabbitMqQueueListener(ConnectionConst.AmqpConnection, ConnectionConst.CommandQueue, false);

            //queueListner.Start().WithMessageAction((message) => {
            //    return ProcessMessage(message);
            //});

            //Console.Read();

            ReceiveMessage();
        }

        static async Task ProcessMessage(IReceivedMessage message)
        {
            var msg = new RabbitSendMessage("CommandResponse", new MessageDto() { Message = "Response", MessageId = message.Header.MessageId, MessageDirection = MessageDirection.ToHub })
            {
                Header = new MessageHeader() { ContentType = MimeTypes.ApplicationJson, CorrelationId = message.Header.CorrelationId },
            };
            var _sender = new RabbitMqQueueSender(ConnectionConst.AmqpConnection, ConnectionConst.Exchange, ConnectionConst.MessageQueue);
            await _sender.Send(msg);
        }

        static void ReceiveMessage()
        {
            var connectionFactory = new ConnectionFactory() { Uri = ConnectionConst.AmqpConnection };
            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

           channel.QueueDeclare(queue: ConnectionConst.DataQueue,
                                                         durable: true,
                                                         exclusive: false,
                                                         autoDelete: false,
                                                         arguments: null);

            Console.WriteLine(" [*] Waiting for logs.");

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: ConnectionConst.DataQueue,
                                noAck: false,
                                consumer: consumer);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                channel.BasicAck(ea.DeliveryTag, false);

                Console.WriteLine(" [x] {0}", message);
            };
           
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}
