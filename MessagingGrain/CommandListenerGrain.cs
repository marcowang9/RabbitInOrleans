using Common;
using Logging.Orleans;
using MessagingGrainInterface;
using Orleans;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using RabbitMQMessaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingGrain
{
    [StorageProvider(ProviderName = "BlobStore")]
    public class CommandListenerGrain : Grain<CommandListenerGrainState>, ICommandListenerGrain, IRemindable
    {
        IOrleansLog _logger;

        TaskScheduler _orleansTaskScheduler;

        public async override Task OnActivateAsync()
        {
            if (_logger == null)
                _logger = OrleansLoggerSingleton.Logger;

            if (!State.IsInitialized)
            {
                if (State.DeviceMappings == null)
                    State.DeviceMappings = new ConcurrentDictionary<string, string>();

                State.DeviceObserverManager = new ObserverSubscriptionManager<IDeviceObserver>();
                State.IsInitialized = true;
            }

            await WriteStateAsync();

            var reminderName = string.Format("{0}", "keepalive");
            await RegisterOrUpdateReminder(reminderName, new TimeSpan(0, 10, 0), new TimeSpan(0, 10, 0));
        }

        public async Task StartCommandProcessorHost(string connectionString, string deviceId)
        {
            _logger.Information(AppendingPrimaryKeyInfoToMessage(string.Format("StartCommandProcessorHost() enter. ConnectionString: {0}", connectionString)));

            await ReadStateAsync();

            _orleansTaskScheduler = TaskScheduler.Current;
            try
            {
                string queueName;
                State.DeviceMappings.TryGetValue(deviceId, out queueName);

                if (string.IsNullOrEmpty(queueName))
                {
                    State.DeviceMappings.TryAdd(deviceId, queueName);
                    var pusher = GrainFactory.GetGrain<IMessagePushGrain>(1);
                    await pusher.NotifyNewDevice(deviceId);
                    // State.DeviceObserverManager.Notify((observer) => observer.NotifyNewDevice(deviceId));
                    await WriteStateAsync();

                    queueName = ConnectionConst.CommandQueuePrefix + deviceId;
                    var _queueListner = new RabbitMqQueueListener(connectionString, queueName);
                    await Task.Run(() => _queueListner.Start(queueName).WithMessageAction(queueName, (message) =>
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

            _logger.Information(AppendingPrimaryKeyInfoToMessage("StartCommandProcessorHost() exit."));
        }

        public async Task SubscribeForNewDevice(IDeviceObserver deviceObserver)
        {
            State.DeviceObserverManager.Subscribe(deviceObserver);
            await WriteStateAsync();
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
