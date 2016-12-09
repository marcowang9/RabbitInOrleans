using Orleans;
using RabbitMQMessaging;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IEventProcessorGrain : IGrainWithStringKey
    {
        Task ProcessMessage(IReceivedMessage message);
    }
}
