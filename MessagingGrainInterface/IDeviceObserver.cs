using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingGrainInterface
{
    public interface IDeviceObserver : IGrainObserver
    {
        void NotifyNewDevice(string deviceId);
    }
}
