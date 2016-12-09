using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagingGrainInterface;
using RabbitMQMessaging;
using Logging.Orleans;
using Orleans;

namespace MessagingGrain
{
    public class PushRouterGrain : Grain, IPushRouterGrain
    {
        private IOrleansLog _logger;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;

            return TaskDone.Done;
        }

        public async Task RouteMessageToPush(IReceivedMessage message)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage("RouteMessageToPush() enter."));

            var pusher = GrainFactory.GetGrain<IMessagePushGrain>(0);
            await pusher.SendMessage(message);

            _logger.Information(AppendingPrimaryKeyInfoToMessage("RouteMessageToClient() exit."));
        }

        private string AppendingPrimaryKeyInfoToMessage(string message)
        {
            return string.Format("{0}_PrimaryKey:{1}.", message, this.GetPrimaryKey());
        }
    }
}
