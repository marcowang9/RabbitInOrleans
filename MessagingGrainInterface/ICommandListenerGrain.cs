using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface ICommandListenerGrain : IGrainWithStringKey
    {
        Task StartCommandProcessorHost(string connectionString, string deviceId);

        Task SubscribeForNewDevice(IDeviceObserver deviceObserver);
    }
}
