using Common;
using Orleans;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IRequestGrain : IGrainWithGuidKey
    {
        Task<ICommandResponseGrain<CommandResult>> RequestForResponse(int messageIndex, string reqMessage, string device);
    }
}
