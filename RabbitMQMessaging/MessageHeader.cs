using Common;
using RabbitMQMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    [Serializable]
    public class MessageHeader
    {
        public string ContentType { get; set; }

        public string CorrelationId { get; set; }

        public string MessageId { get; set; }

        public string ReplyTo { get; set; }

        public string ReplyToSessionId { get; set; }

        public DateTime ScheduledEnqueueTimeUtc { get; set; }

        public string SessionId { get; set; }

        public TimeSpan TimeToLive { get; set; }

        public string To { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "{0} - MessageId={1} CorrelationId={2} ReplyTo={3} ReplyToSessionId={4} SessionId={5} To={6} ContentType={7}",
                    GetType().Name,
                    MessageId ?? "",
                    CorrelationId ?? "",
                    ReplyTo ?? "",
                    ReplyToSessionId ?? "",
                    SessionId ?? "",
                    To ?? "",
                    ContentType ?? "");
        }
    }
}
