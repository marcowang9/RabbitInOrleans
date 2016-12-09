using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public interface ISendMessage
    {
        MessageHeader Header { get; }

        IDictionary<string, object> Properties { get; }

        object GetDto();

        Type GetDtoType();
    }
}
