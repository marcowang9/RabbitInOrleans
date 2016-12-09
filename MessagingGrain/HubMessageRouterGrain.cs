using System.Threading.Tasks;
using Orleans;
using MessagingGrainInterface;
using RabbitMQMessaging;
using System;
using Logging.Orleans;
using Common;

namespace MessagingGrain
{
    public class HubMessageRouterGrain : Grain, IHubMessageRouterGrain
    {
        private IOrleansLog _logger;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;
        
            return TaskDone.Done;
        }

        public async Task RouteMessageToHub(IReceivedMessage message)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage("RouteMessageToClient() enter."));

            var header = message.Header;
            string correlationId = header.CorrelationId;

            Guid correlationGuid;
            if (Guid.TryParse(correlationId, out correlationGuid) == false)
            {
                _logger.Error("Error when parse CorrelationId: {0}", new object[] { correlationId });

                await TaskDone.Done;
            }

            var cmdResponseGrain =
             GrainFactory.GetGrain<ICommandResponseGrain<CommandResult>>(correlationGuid);

            await cmdResponseGrain.SetResponseAsync("Request", message);

            _logger.Information(AppendingPrimaryKeyInfoToMessage("RouteMessageToClient() exit."));
        }

        private string AppendingPrimaryKeyInfoToMessage(string message)
        {
            return string.Format("{0}_PrimaryKey:{1}.", message, this.GetPrimaryKey());
        }
    }
}
