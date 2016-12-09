using Orleans;
using RabbitMQMessaging;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IPushRouterGrain : IGrainWithGuidKey
    {
        Task RouteMessageToPush(IReceivedMessage message);
    }
}
