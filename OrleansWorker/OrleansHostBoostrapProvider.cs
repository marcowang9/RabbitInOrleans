using Common;
using Logging.Orleans;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansWorker
{
    class OrleansHostBoostrapProvider : CompositeBootstrapProviderBase
    {
        public OrleansHostBoostrapProvider() : 
            base(GetLoggerProvider(), 
                    GetEventProcessorHostProvider())
        {
        }

        private static IBootstrapProvider GetLoggerProvider()
        {
            return new OrleansLoggerProvider("OrleansLoggerProvider");
        }

        //private static IBootstrapProvider GetDiBoostrapProvider()
        //{
        //    return new DependencyRegistrationProvider("DependencyRegistrationProvider");
        //}

        private static IBootstrapProvider GetEventProcessorHostProvider()
        {
            return new EventProcessorHostProvider("EventProcessorHostProvider", ConnectionConst.AmqpConnection, ConnectionConst.MessageQueue, ConnectionConst.DataQueue);
        }
    }
}
