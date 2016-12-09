using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    [Serializable]
    public class MessageDto
    {
        public string Message { get; set; }
        public string MessageId { get; set; }
        public MessageDirection MessageDirection { get; set; }
        public string DeviceId { get; set; }
    }
}
