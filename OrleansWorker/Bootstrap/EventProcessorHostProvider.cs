using MessagingGrainInterface;
using Orleans.Providers;
using System;
using System.Threading.Tasks;

namespace OrleansWorker
{
    class EventProcessorHostProvider : IBootstrapProvider
    {
        private readonly string _connection;
        //private readonly string _commandQueueName;
        private readonly string _messageQueueName;
        private readonly string _dataQueueName;

        public string Name { get; private set; }

        public EventProcessorHostProvider(string name, string connectionString, string messageQueueName, string dataQueueName)
        {
            Name = name;
            _connection = connectionString;
            //_commandQueueName = commandQueueName;
            _messageQueueName = messageQueueName;
            _dataQueueName = dataQueueName;
        }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            //var commandEventProcessorGrain = providerRuntime.GrainFactory.GetGrain<IEventProcessorHostGrain>(_commandQueueName);
            //await commandEventProcessorGrain.StartEventProcessorHost(_connection, _commandQueueName);

            //var messageEventProcessorGrain = providerRuntime.GrainFactory.GetGrain<IEventProcessorHostGrain>(_messageQueueName);
            //await messageEventProcessorGrain.StartEventProcessorHost(_connection, _messageQueueName);

            //var dataEventProcessorGrain = providerRuntime.GrainFactory.GetGrain<IEventProcessorHostGrain>(_dataQueueName);
            //await messageEventProcessorGrain.StartEventProcessorHost(_connection, _dataQueueName);

            var eventProcessorGrain = providerRuntime.GrainFactory.GetGrain<IEventProcessorHostGrain>(Guid.NewGuid().ToString());
            await eventProcessorGrain.StartEventProcessorHost(_connection, _messageQueueName, _dataQueueName);
        }
    }
}
