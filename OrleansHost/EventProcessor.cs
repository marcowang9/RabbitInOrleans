using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessagingGrainInterface;
using Orleans.Runtime.Configuration;
using RabbitMQMessaging;
using Logging.Orleans;
using Common;

namespace OrleansHost
{
    public class EventProcessor : IDisposable
    {
        IFluentQueueListener _queueListener;

        IConnectionFactory _connectionFactory;

        IConnection _connection;

        IModel _model;

        public void  StartProcessing()
        {
            try
            {
                GrainClient.Initialize("ClientOrleansConfiguration.xml");
              
                Task.Run(() =>
                {
                    _queueListener = new RabbitMqQueueListener(ConnectionConst.AmqpConnection, ConnectionConst.CommandQueue, true);
                    var processor = GrainClient.GrainFactory.GetGrain<IEventProcessorGrain>(ConnectionConst.CommandQueue);
                    _queueListener.Start().WithMessageAction((message) =>
                    {
                        return processor.ProcessMessage(message);
                    });
                });
            }
            catch (Exception ex)
            {
            }

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            _model.Close();
            _model.Dispose();
            _connection.Close();
            _connection.Dispose();
            
        }
    }
}
