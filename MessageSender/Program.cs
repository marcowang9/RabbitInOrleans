using Common;
using MessagingGrainInterface;
using Orleans;
using RabbitMQ.Client;
using RabbitMQMessaging;
using System;
using System.Text;
using System.Threading;

namespace MessageSender
{
    class Program
    {
        static void Main(string[] args)
        {
            SendDirectMessage();

            //GrainClient.Initialize("ClientOrleansConfiguration.xml");
            //var sender = GrainClient.GrainFactory.GetGrain<ISenderGrain>(Guid.NewGuid());

            //ulong index = 0;
            //do
            //{
            //    var message = new RabbitSendMessage("Hello", new MessageDto() { Message = "Hello " + index++, MessageDirection = MessageDirection.ToClient })
            //    {
            //        Header = new MessageHeader() { ContentType = MimeTypes.ApplicationJson, Direction = MessageDirection.ToClient }
            //    };
            //    sender.SendMessage(message, ConnectionConst.AmqpConnection, ConnectionConst.Exchange, ConnectionConst.CommandQueue);

            //    Console.WriteLine(((MessageDto)(message.GetDto())).Message);
            //    Thread.Sleep(1000);
            //} while (index < 100);

            Console.Read();
        }

        static void SendDirectMessage()
        {
            var connectionFactory = new ConnectionFactory() { Uri = ConnectionConst.AmqpConnection };

            double index = 0;
            Console.WriteLine("Press Enter to Exit");

            try
            {
                using (var connection = connectionFactory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        Console.Write("Connnected!" + ConnectionConst.AmqpConnection);
                        channel.ExchangeDeclare(exchange: "mytest", type: "direct", durable: true, autoDelete: false, arguments: null);
                        channel.QueueDeclare(queue: "myqueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        do
                        {
                            channel.BasicPublish(exchange: "",
                                        routingKey: ConnectionConst.CommandQueue,
                                        basicProperties: properties,
                                        body: Encoding.UTF8.GetBytes(string.Format("Hello {0}", index)));
                            Console.WriteLine(string.Format("Message sent. {0}", index++));
                            Thread.Sleep(1000);
                        }
                        while (true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            
        }
    }
}
