using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using MessagingGrainInterface;
using RabbitMQMessaging;
using System;
using RabbitMQ.Client;
using Logging.Orleans;
using System.Text;

namespace MessagingGrain
{
    /// <summary>
    /// Grain implementation class IRabbitMQSenderGrain.
    /// </summary>
    public class RabbitMQSenderGrain : Grain, ISenderGrain
    {
        IOrleansLog _logger;
        IFluentQueueSender _sender;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
            {
                _logger = OrleansLoggerSingleton.Logger;
            }
            return TaskDone.Done;
        }

        public async Task SendMessage(ISendMessage message, string connectionString, string exchange, string routingKey)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage("RabbitMQSenderGrain.SendMessage() enter"));

            using (_sender = new RabbitMqQueueSender(connectionString, exchange, routingKey))
            {
                await _sender.Send(message);
            }
            _logger.Information(AppendingPrimaryKeyInfoToMessage("RabbitMQSenderGrain.SendMessage() exit"));
        }

        public override Task OnDeactivateAsync()
        {
            return TaskDone.Done;
        }

        private string AppendingPrimaryKeyInfoToMessage(string message)
        {
            return string.Format("{0}_PrimaryKey:{1}.", message, "");
        }
    }
}
