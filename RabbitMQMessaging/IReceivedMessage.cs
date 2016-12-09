using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public interface IReceivedMessage
    {
        MessageHeader Header { get; }

        IDictionary<string, object> Properties { get; }

        Task AbandonAsync();

        Task CompleteAsync();

        byte[] GetBody();

        MessageDirection Direction { get; }
    }
}
