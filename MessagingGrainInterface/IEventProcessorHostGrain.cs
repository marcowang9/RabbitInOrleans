using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IEventProcessorHostGrain : IGrainWithStringKey
    {
        Task StartEventProcessorHost(string connectionString, params string[] queueNames);
    }
}
