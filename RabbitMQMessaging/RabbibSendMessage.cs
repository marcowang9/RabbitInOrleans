using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    [Serializable]
    public class RabbitSendMessage : ISendMessage
    {
        public MessageHeader Header { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        private readonly byte[] _body;

        private readonly object _dto;

        private readonly Type _dtoType;

        public RabbitSendMessage(string message, object dto)
        {
            _body = Encoding.UTF8.GetBytes(message);
            _dto = dto;
            _dtoType = dto.GetType();
        }

        public object GetDto()
        {
            return _dto;
        }

        public Type GetDtoType()
        {
            return _dtoType;
        }
    }
}
