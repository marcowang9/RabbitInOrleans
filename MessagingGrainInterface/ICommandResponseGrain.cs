using System.Threading.Tasks;
using Orleans;

namespace MessagingGrainInterface
{
    public interface ICommandResponseGrain<T> : IGrainWithGuidKey where T : class
    {
        Task SetResponseAsync(string commandName, object commandResultValue);

        Task<bool> IsCompleteAsync();

        Task<T> GetResultAsync();
    }
}
