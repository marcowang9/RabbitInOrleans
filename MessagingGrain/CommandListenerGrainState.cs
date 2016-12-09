using MessagingGrainInterface;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingGrain
{
    public class CommandListenerGrainState : GrainState
    {
        public bool IsInitialized { get; set; }

        public ConcurrentDictionary<string, string> DeviceMappings { get; set; }

        public ObserverSubscriptionManager<IDeviceObserver> DeviceObserverManager { get; set; }
    }
}
