using Common;
using RabbitMQMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebClient.Controllers
{
    public class ResponseController : ApiController
    {
        IFluentQueueSender _sender;
       
        [HttpPost]
        public async Task PostAsync(string messageId, string respMessage)
        {
            var message = new RabbitSendMessage("CommandResponse", new MessageDto() { Message = respMessage, MessageId = messageId.ToString(), MessageDirection = MessageDirection.ToHub })
            {
                Header = new MessageHeader() { ContentType = MimeTypes.ApplicationJson,  CorrelationId= messageId },
            };
            using (_sender = new RabbitMqQueueSender(ConnectionConst.AmqpConnection, ConnectionConst.Exchange, ConnectionConst.MessageQueue))
            {
                await _sender.Send(message);
            }
        }
    }
}
