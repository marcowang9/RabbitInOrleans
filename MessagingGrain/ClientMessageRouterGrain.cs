using System.Threading.Tasks;
using Orleans;
using MessagingGrainInterface;
using RabbitMQMessaging;
using System;
using Logging.Orleans;
using System.Text;

namespace MessagingGrain
{
    public class ClientMessageRouterGrain : Grain, IClientMessageRouterGrain
    {
        private IOrleansLog _logger;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;

            return TaskDone.Done;
        }

        public async Task RouteMessageToClient(IReceivedMessage message)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage("RouteMessageToClient() enter."));

            var notifier = GrainFactory.GetGrain<IPushNotifierGrain>(0);
            await notifier.SendMessage(message);

            _logger.Information(AppendingPrimaryKeyInfoToMessage("RouteMessageToClient() exit."));
        }

        private string AppendingPrimaryKeyInfoToMessage(string message)
        {
            return string.Format("{0}_PrimaryKey:{1}.", message, this.GetPrimaryKey());
        }
    }
}
