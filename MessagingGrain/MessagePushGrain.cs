using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagingGrainInterface;
using Orleans;
using RabbitMQMessaging;
using Orleans.Concurrency;
using Microsoft.AspNet.SignalR.Client;
using Common;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MessagingGrain
{
    [Reentrant]
    [StatelessWorker]
    public class MessagePushGrain : Grain, IMessagePushGrain
    {
        Dictionary<string, Tuple<HubConnection, IHubProxy>> hubs = new Dictionary<string, Tuple<HubConnection, IHubProxy>>();

        public override async Task OnActivateAsync()
        {
            if (AzureEnvironment.IsInAzure)
            {
                // in azure
                await RefreshHubs(null);
                // set up a timer to regularly refresh the hubs, to respond to azure infrastructure changes
                this.RegisterTimer(RefreshHubs, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            }
            else
            {
                // not in azure, the SignalR hub is running locally
                await AddHub("http://localhost:61984/");
            }

            await base.OnActivateAsync();
        }

        private async Task AddHub(string address)
        {
            // create a connection to a hub
            var hubConnection = new HubConnection(address);
            hubConnection.Headers.Add("ORLEANS", "GRAIN");
            var hub = hubConnection.CreateHubProxy("SignalRHub");
            await hubConnection.Start();
            hubs.Add(address, new Tuple<HubConnection, IHubProxy>(hubConnection, hub));
        }

        private async Task RefreshHubs(object p)
        {
            var addresses = new List<string>();
            var tasks = new List<Task>();

            // discover the current infrastructure
            foreach (var instance in RoleEnvironment.Roles["WebHub"].Instances)
            {
                var endpoint = instance.InstanceEndpoints["InternalSignalR"];
                addresses.Add(string.Format("http://{0}", endpoint.IPEndpoint.ToString()));
            }
            var newHubs = addresses.Where(x => !hubs.Keys.Contains(x)).ToArray();
            var deadHubs = hubs.Keys.Where(x => !addresses.Contains(x)).ToArray();

            // remove dead hubs
            foreach (var hub in deadHubs)
            {
                hubs.Remove(hub);
            }

            // add new hubs
            foreach (var hub in newHubs)
            {
                tasks.Add(AddHub(hub));
            }
            await Task.WhenAll(tasks);
        }

        public Task SendMessage(IReceivedMessage message)
        {
            var messagesToSend = Encoding.UTF8.GetString(message.GetBody());

            var promises = new List<Task>();
            foreach (var hub in hubs.Values)
            {
                try
                {
                    if (hub.Item1.State == ConnectionState.Connected)
                    {
                        hub.Item2.Invoke("MessageUpdate", messagesToSend);
                    }
                    else
                    {
                        hub.Item1.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return TaskDone.Done;
        }

        public Task NotifyNewDevice(string deviceId)
        {
            foreach (var hub in hubs.Values)
            {
                try
                {
                    if (hub.Item1.State == ConnectionState.Connected)
                    {
                        hub.Item2.Invoke("DeviceUpdate", deviceId);
                    }
                    else
                    {
                        hub.Item1.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return TaskDone.Done;
        }
    }
}
