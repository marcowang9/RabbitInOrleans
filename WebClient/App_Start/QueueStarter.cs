using Common;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebClient
{
    public static class QueueStarter
    {
        public static void RegisterQueue(string deviceId)
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