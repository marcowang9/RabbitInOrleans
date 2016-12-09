using Common;
using RabbitMQMessaging;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebClient.Controllers
{
    public class DataPushController : ApiController
    {
        IFluentQueueSender _sender;

        [HttpPost]
        public async Task PostAsync(string dataMessage)
        {
            var message = new RabbitSendMessage("DataPush", new MessageDto() { Message = dataMessage, MessageDirection=MessageDirection.ToPush })
            {
                Header = new MessageHeader() { ContentType = MimeTypes.ApplicationJson },
            };
            using (_sender = new RabbitMqQueueSender(ConnectionConst.AmqpConnection, ConnectionConst.Exchange, ConnectionConst.DataQueue))
            {
                await _sender.Send(message);
            }
        }
    }
}
