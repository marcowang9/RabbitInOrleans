using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public  interface IFluentQueueListener : IDisposable
    {
        IFluentQueueListener Start(params string[] queueNames);

        IFluentQueueListener WithMessageAction(string queueName, Func<IReceivedMessage, Task> messageAction);

        IFluentQueueListener WithMessageAction(string[] queueNames, Func<string, IReceivedMessage, Task> messageAction);
    }
}
