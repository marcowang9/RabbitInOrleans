using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace RabbitMQMessaging
{
    public static class MessageToRabbitMqMessageConvertor
    {
        private const byte PersistentDeliveryMode = 2;

        public static RabbitMessage ConvertMessageToRabbitMessage(ISendMessage message, IModel model)
        {
            var rabbitMessage = CreateMessage(message.GetDto(), message.GetDtoType(), message.Header.ContentType, model);
            AddMessageDetailsToMessage(message.Header, rabbitMessage);
            return rabbitMessage;
        }

        private static void AddMessageDetailsToMessage(MessageHeader messageHeader, RabbitMessage sendMessage)
        {
            AssignIfNotNullOrEmpty(() => messageHeader.MessageId, x => sendMessage.BasicProperties.MessageId = x);
            AssignIfNotNullOrEmpty(() => messageHeader.ReplyTo, x => sendMessage.BasicProperties.ReplyTo = x);
            AssignIfNotNullOrEmpty(
                () => messageHeader.CorrelationId,
                x => sendMessage.BasicProperties.CorrelationId = x);
            AssignIfNotNullOrEmpty(() => messageHeader.TimeToLive, x => sendMessage.TimeToLive = x);
        }

        private static void AssignIfNotNullOrEmpty(Func<string> input, Action<string> onNotNullOrEmptyAction)
        {
            if (string.IsNullOrEmpty(input()))
            {
                return;
            }
            onNotNullOrEmptyAction(input());
        }

        private static void AssignIfNotNullOrEmpty(Func<TimeSpan> input, Action<TimeSpan> onNotNullOrEmptyAction)
        {
            var timeSpan = input();
            if (timeSpan == default(TimeSpan))
            {
                return;
            }

            onNotNullOrEmptyAction(timeSpan);
        }

        private static RabbitMessage CreateCompressedMessage(string bodyString, Type dtoType, IModel model)
        {
            var bodyStream = GZipper.Zip(bodyString);
            var message = new RabbitMessage(model)
            {
                Body = bodyStream,
                BasicProperties =
                {
                    Type = dtoType.ToString(),
                    ContentType = MimeTypes.ApplicationJsonPlusGzip,
                    ContentEncoding = "gzip",
                    DeliveryMode = PersistentDeliveryMode
                },
            };

            message.BasicProperties.Headers.Add(
                CommonMessagePropertyNames.ContentType,
                MimeTypes.ApplicationJsonPlusGzip);
            return message;
        }

        private static RabbitMessage CreateMessage(object dto, Type dtoType, string contentType, IModel model)
        {
            var bodyString = JsonConvert.SerializeObject(dto);
            if (contentType == MimeTypes.ApplicationJson)
            {
                return CreateTextMessage(bodyString, dtoType, model);
            }

            return CreateCompressedMessage(bodyString, dtoType, model);
        }

        private static RabbitMessage CreateTextMessage(string bodyString, Type dtoType, IModel model)
        {
            var rabbitMessage = new RabbitMessage(model)
            {
                Body = Encoding.UTF8.GetBytes(bodyString),
                BasicProperties =
                {
                    Type = dtoType.ToString(),
                    ContentType = MimeTypes.ApplicationJson,
                    ContentEncoding = Encoding.UTF8.EncodingName,
                    DeliveryMode = PersistentDeliveryMode
                }
            };
            rabbitMessage.BasicProperties.Headers.Add(CommonMessagePropertyNames.ContentType, MimeTypes.ApplicationJson);
            return rabbitMessage;
        }

    }
}
