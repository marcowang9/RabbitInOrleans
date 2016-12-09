using Orleans;
using RabbitMQMessaging;
using System;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
	public interface IHubMessageRouterGrain : IGrainWithGuidKey
    {
        Task RouteMessageToHub(IReceivedMessage message);
    }
}
