using System;
using System.Threading.Tasks;
using Common;
using Orleans;
using Logging.Orleans;
using RabbitMQMessaging;
using MessagingGrainInterface;

namespace MessagingGrain
{
    public class RequestGrain : Grain, IRequestGrain
    {
        private IOrleansLog _logger;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;

            return TaskDone.Done;
        }

        public async Task<ICommandResponseGrain<CommandResult>> RequestForResponse(int messageIndex, string reqMessage, string device)
        {
            _logger.MemberEntry();

            var messageId = Guid.NewGuid();

            var sender = GrainFactory.GetGrain<ISenderGrain>(Guid.NewGuid());

            var message = new RabbitSendMessage(messageIndex.ToString(), new MessageDto() { Message = reqMessage, MessageId= messageId.ToString(), MessageDirection = MessageDirection.ToClient, DeviceId = device })
            {
                Header = new MessageHeader() { ContentType = MimeTypes.ApplicationJson, CorrelationId = messageId.ToString() },
            };
            await sender.SendMessage(message, ConnectionConst.AmqpConnection, ConnectionConst.CommandExchange, device);

            var cmdResponse = GrainFactory.GetGrain<ICommandResponseGrain<CommandResult>>(messageId);
            _logger.MemberExit();
            return cmdResponse;
        }
    }
}
