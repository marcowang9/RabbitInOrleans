using Orleans;
using RabbitMQMessaging;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IMessagePushGrain : IGrainWithIntegerKey
    {
        Task SendMessage(IReceivedMessage message);

        Task NotifyNewDevice(string deviceId);
    }
}
