using Orleans;
using RabbitMQMessaging;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IPushNotifierGrain : IGrainWithIntegerKey
    {
        Task SendMessage(IReceivedMessage message);
    }
}
