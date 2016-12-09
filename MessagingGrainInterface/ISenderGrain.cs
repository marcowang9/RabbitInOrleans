using System.Threading.Tasks;
using Orleans;
using RabbitMQ.Client;
using RabbitMQMessaging;

namespace MessagingGrainInterface
{
    /// <summary>
    /// Grain interface IGrain1
    /// </summary>
	public interface ISenderGrain : IGrainWithGuidKey
    {
        Task SendMessage(ISendMessage message, string connectionString, string exchange, string routingKey);
    }
}
