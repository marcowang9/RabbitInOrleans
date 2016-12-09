using Logging.Orleans;
using MessagingGrainInterface;
using Orleans;
using Orleans.Concurrency;
using RabbitMQMessaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Providers;
using System.Collections.Concurrent;

namespace MessagingGrain
{
    [StorageProvider(ProviderName = "BlobStore")]
    public class EventProcessorHostGrain : Grain, IEventProcessorHostGrain, IRemindable
    {
        IOrleansLog _logger;

        IFluentQueueListener _queueListner;

        TaskScheduler _orleansTaskScheduler;

        public override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;

            return TaskDone.Done;
        }

        public async Task StartEventProcessorHost(string connectionString, params string[] queueNames)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage(string.Format("StartEventProcessorHost() enter. ConnectionString: {0}", connectionString)));

            _orleansTaskScheduler = TaskScheduler.Current;
            try
            {
                if (_queueListner == null)
                {
                    var reminderName = string.Format("{0}", "keepalive");
                    await RegisterOrUpdateReminder(reminderName, new TimeSpan(0, 10, 0), new TimeSpan(0, 10, 0));

                    _queueListner = new RabbitMqQueueListener(connectionString, queueNames);
                    await Task.Run(() => _queueListner.Start(queueNames).WithMessageAction(queueNames, (queueName, message) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            var processor = GrainFactory.GetGrain<IEventProcessorGrain>(queueName);
                            return processor.ProcessMessage(message);
                        }, CancellationToken.None, TaskCreationOptions.None, _orleansTaskScheduler);
                    }));
                }
            }
            catch (RabbitMQException rabbitmqEx)
            {
                _logger.Error(rabbitmqEx);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            _logger.Information(AppendingPrimaryKeyInfoToMessage("StartEventProcessorHost() exit."));
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }

        private string AppendingPrimaryKeyInfoToMessage(string message)
        {
            return string.Format("{0}_PrimaryKey:{1}.", message, this.GetPrimaryKeyString());
        }

        Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            return TaskDone.Done;
        }
    }
}
