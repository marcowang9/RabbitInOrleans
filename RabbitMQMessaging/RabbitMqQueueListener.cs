using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RabbitMQMessaging
{
    public class RabbitMqQueueListener : IFluentQueueListener
    {
        readonly string _connectionString;

        IConnectionFactory _connectionFactory;

        public ConcurrentDictionary<string, IModel> QueueModels
        {
            get { return _queueModels.Value; }
        }

        Lazy<ConcurrentDictionary<string, IModel>> _queueModels = new Lazy<ConcurrentDictionary<string, IModel>>(()=> new ConcurrentDictionary<string, IModel>());

        Lazy<ConcurrentDictionary<string, IConnection>> _queueConnections = new Lazy<ConcurrentDictionary<string, IConnection>>(() => new ConcurrentDictionary<string, IConnection>());

        Lazy<ConcurrentDictionary<string, EventingBasicConsumer>> _queueConsumers = new Lazy<ConcurrentDictionary<string, EventingBasicConsumer>>(() =>new ConcurrentDictionary<string, EventingBasicConsumer>());

        //IModel _model;

        public RabbitMqQueueListener(string connectionString, params string[] queueNames)
        {
            _connectionString = connectionString;
            InitializeConnection();
            InitializeQueueModels(queueNames);
        }

        private void InitializeQueueModels(string[] queueNames)
        {
            try
            {
                Parallel.ForEach(queueNames, (queueName) =>
                {
                    IConnection connection = _queueConnections.Value.GetOrAdd(queueName, _connectionFactory.CreateConnection());
                    _queueModels.Value.AddOrUpdate(queueName, connection.CreateModel(), (key, existing)=> { return existing; });
                });

                //for (int i = 0; i < queueNames.Length; i++)
                //{
                //    string queueName = queueNames[i];
                //    IConnection connection = _queueConnections.Value.GetOrAdd(queueName, _connectionFactory.CreateConnection());
                //    _queueModels.Value.AddOrUpdate(queueName, connection.CreateModel(), null);
                //}
            }
            catch (Exception ex)
            {
                throw new RabbitMQException("RabbitMQ Connection Error", ex);
            }
            
        }

        private void InitializeConnection()
        {
            try
            {
                if (_connectionFactory == null)
                    _connectionFactory = new ConnectionFactory() { Uri = _connectionString };
                       
            }
            catch (Exception ex)
            {
                Dispose();
                throw new RabbitMQException("RabbitMQ Connection Error", ex);
            }
        }

        public IFluentQueueListener Start(params string[] queueNames)
        {
            try
            {
                Parallel.ForEach(queueNames, (queueName) =>
                {
                    IModel model = _queueModels.Value[queueName];
                    model.QueueDeclare(queue: queueName,
                                                             durable: true,
                                                             exclusive: false,
                                                             autoDelete: false,
                                                             arguments: null);
                    _queueConsumers.Value.AddOrUpdate(queueName, new EventingBasicConsumer(model), (key, existing)=> { return existing; });
                });
            }
            catch (Exception ex)
            {
                Dispose();
                throw new RabbitMQException("RabbitMQ Connection Error", ex);
            }
            return this;
        }

        public IFluentQueueListener WithMessageAction(string queueName, Func<IReceivedMessage, Task> messageAction)
        {
            try
            {
                if (_queueConsumers.Value.ContainsKey(queueName) && _queueModels.Value.ContainsKey(queueName))
                {
                    var consumer = _queueConsumers.Value[queueName];
                    var model = _queueModels.Value[queueName];
                    consumer.Received += async (m, e) =>
                    {
                        var rabbitMessage = new RabbitReceivedMessage(model, e)
                        {
                            BasicProperties = e.BasicProperties,
                            Body = e.Body
                        };
                        await ProcessMessage(rabbitMessage, messageAction);
                    };
                    model.BasicConsume(queueName, false, consumer);
                }
            }
            catch (Exception ex)
            {
                throw new RabbitMQException("RabbitMQ Connection Error", ex);
            }
            return this;
        }

        public IFluentQueueListener WithMessageAction(string[] queueNames, Func<string, IReceivedMessage, Task> messageAction)
        {
            try
            {
                Parallel.ForEach(queueNames, (queueName) =>
                {
                    if (_queueConsumers.Value.ContainsKey(queueName) && _queueModels.Value.ContainsKey(queueName))
                    {
                        var consumer = _queueConsumers.Value[queueName];
                        var model = _queueModels.Value[queueName];
                        consumer.Received += async (m, e) =>
                        {
                            var rabbitMessage = new RabbitReceivedMessage(model, e)
                            {
                                BasicProperties = e.BasicProperties,
                                Body = e.Body
                            };
                            await ProcessMessage(rabbitMessage, queueName, messageAction);
                        };
                        model.BasicConsume(queueName, false, consumer);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new RabbitMQException("RabbitMQ Connection Error", ex);
            }
            return this;
        }

        public void Close()
        {
            Parallel.ForEach(_queueConnections.Value, (connection) => { connection.Value.Close(); });
            Parallel.ForEach(_queueModels.Value, (model) => { model.Value.Close(); });
        }

        private async Task ProcessMessage(IReceivedMessage rabbitMessage, Func<IReceivedMessage, Task> messageAction)
        {
            try
            {
                await messageAction(rabbitMessage);
                await rabbitMessage.CompleteAsync();
            }
            catch (Exception exception)
            {
                try
                {
                    await rabbitMessage.AbandonAsync();
                }
                catch (Exception abandonException)
                {
                    throw new RabbitMQException("Received Message Abandon Error", abandonException);
                }
                throw new RabbitMQException("Received Message Completion Error", exception);
            }
        }

        private async Task ProcessMessage(IReceivedMessage rabbitMessage, string queueName, Func<string, IReceivedMessage, Task> messageAction)
        {
            try
            {
                await messageAction(queueName, rabbitMessage);
                await rabbitMessage.CompleteAsync();
            }
            catch (Exception exception)
            {
                try
                {
                    await rabbitMessage.AbandonAsync();
                }
                catch (Exception abandonException)
                {
                    throw new RabbitMQException("Received Message Abandon Error", abandonException);
                }
                throw new RabbitMQException("Received Message Completion Error", exception);
            }
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
                Parallel.ForEach(_queueModels.Value, (model) => { model.Value.Dispose(); });
                Parallel.ForEach(_queueConnections.Value, (connection) => { connection.Value.Dispose(); });
                GC.SuppressFinalize(_queueConnections);
                GC.SuppressFinalize(_queueModels);
            }
        }
    }
}
