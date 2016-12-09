using System;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public interface IFluentQueueSender : IDisposable
    {
        Task Send(ISendMessage message);
    }
}