using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public enum MessageDirection
    {
        ToClient = 0,
        ToHub = 1,
        ToPush = 2,
        ToStart = 3
    }
}
