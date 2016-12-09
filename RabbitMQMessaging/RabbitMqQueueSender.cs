using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public class RabbitMqQueueSender : IFluentQueueSender
    {
        readonly string _routingKey;

        readonly string _exchange;

        readonly string _connectionString;

        IConnectionFactory _connectionFactory;

        IConnection _connection;

        IModel _model;

        public RabbitMqQueueSender(string connectionString, string exchange, string routingKey)
        {
            _connectionString = connectionString;
            _exchange = exchange;
            _routingKey = routingKey;

            InitializeConnection();
        }

        public async Task Send(ISendMessage message)
        {
            var rabbitMessage = MessageToRabbitMqMessageConvertor.ConvertMessageToRabbitMessage(message, _model);
            await Task.Run(() => {
                    _model.BasicPublish(_exchange, _routingKey, rabbitMessage.BasicProperties, rabbitMessage.Body);
            });
        }

        private void InitializeConnection()
        {
            try
            {
                if (_connectionFactory == null)
                    _connectionFactory = new ConnectionFactory() { Uri = _connectionString };

                if (_connection == null)
                    _connection = _connectionFactory.CreateConnection();

                if (_model == null)
                    _model = _connection.CreateModel();

                //_model.QueueDeclare(queue: _routingKey, durable: true, exclusive: false, autoDelete: false, arguments: null);
            }
            catch (Exception ex)
            {
                Dispose();
                throw new RabbitMQException("RabbitMQ Connection Error", ex);
            }
        }

        public void Close()
        {
            _model.Close();
            _connection.Close();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                Close();
                _model.Dispose();
                _connection.Dispose();
            }
        }
    }
}
