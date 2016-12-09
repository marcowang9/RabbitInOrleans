using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace OrleansWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private AzureSilo _silo;

        public override void Run()
        {
            Trace.TraceInformation("OrleansWorker is running");

            try
            {
                var config = new ClusterConfiguration();
                config.StandardLoad();
                ConfigureStorageProvider(config);
                _silo = new AzureSilo();
                var isSiloStarted = _silo.Start(config);
                if (isSiloStarted == false)
                {
                    Trace.TraceError("OrleansWorker.Run() -Error in starting silo");
                }
                _silo.Run();
            }
            finally
            {
                this._manualResetEvent.Set();
            }
        }

        private void ConfigureStorageProvider(ClusterConfiguration config)
        {
            var properties = new Dictionary<string, string>();
            var connectionString = CloudConfigurationManager.GetSetting("DataConnectionString");
            properties.Add("DataConnectionString", connectionString);

            config.Globals.RegisterStorageProvider("Orleans.StorageProvider.Blob.AzureBlobStorage", "BlobStore", properties);
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("OrleansWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("CloudPlatform.Tenant.Worker.OrleansHost is stopping");
            _silo.Stop();
            _manualResetEvent.Set();

            base.OnStop();

            Trace.TraceInformation("CloudPlatform.Tenant.Worker.OrleansHost has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
