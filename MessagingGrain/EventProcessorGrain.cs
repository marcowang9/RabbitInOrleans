using System.Threading.Tasks;
using Orleans;
using MessagingGrainInterface;
using RabbitMQMessaging;
using System;
using System.Text;
using Logging.Orleans;
using Common;

namespace MessagingGrain
{
    /// <summary>
    /// Grain implementation class EventProcessorGrain.
    /// </summary>
    public class EventProcessorGrain : Grain, IEventProcessorGrain
    {
        IOrleansLog _logger;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;

            return TaskDone.Done;
        }

        public async Task ProcessMessage(IReceivedMessage message)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage("ProcessMessage() enter."));
            string str = Encoding.UTF8.GetString(message.GetBody());

            var msg = JsonHelper.DeserializeJsonToObject<MessageDto>(str);

            if (msg.MessageDirection == MessageDirection.ToClient)
            {
                var clientRouterGrain = GrainFactory.GetGrain<IClientMessageRouterGrain>(Guid.NewGuid());
                await clientRouterGrain.RouteMessageToClient(message);
            }

            if (msg.MessageDirection == MessageDirection.ToHub)
            {
                var hubRouterGrain = GrainFactory.GetGrain<IHubMessageRouterGrain>(Guid.NewGuid());
                await hubRouterGrain.RouteMessageToHub(message);
            }

            if (msg.MessageDirection == MessageDirection.ToPush)
            {
                var pushRouterGrain = GrainFactory.GetGrain<IPushRouterGrain>(Guid.NewGuid());
                await pushRouterGrain.RouteMessageToPush(message);
            }

            if (msg.MessageDirection == MessageDirection.ToStart)
            {
                var commandListenerGrain = GrainFactory.GetGrain<ICommandListenerGrain>("CommandListener");
                await commandListenerGrain.StartCommandProcessorHost(ConnectionConst.AmqpConnection, msg.Message);
            }

            _logger.Information(string.Format("Message received: {0}",str));
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }

        private string AppendingPrimaryKeyInfoToMessage(string message)
        {
            return string.Format("{0}_PrimaryKey:{1}.", message, this.GetPrimaryKeyString());
        }
    }
}
