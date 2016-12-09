using Orleans;
using RabbitMQMessaging;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IClientMessageRouterGrain : IGrainWithGuidKey
    {
        Task RouteMessageToClient(IReceivedMessage message);
    }
}
