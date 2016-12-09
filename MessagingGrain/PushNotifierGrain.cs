using Common;
using MessagingGrainInterface;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans;
using Orleans.Concurrency;
using RabbitMQMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingGrain
{
    [Reentrant]
    [StatelessWorker]
    public class PushNotifierGrain : Orleans.Grain, IPushNotifierGrain
    {
        Dictionary<string, Tuple<HubConnection, IHubProxy>> hubs = new Dictionary<string, Tuple<HubConnection, IHubProxy>>();
        List<string> messageQueue = new List<string>();

        public override async Task OnActivateAsync()
        {
            // set up a timer to regularly flush the message queue
            this.RegisterTimer(FlushQueue, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

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
                await AddHub("http://localhost:61288/");
            }

            await base.OnActivateAsync();
        }

        private async Task RefreshHubs(object _)
        {
            var addresses = new List<string>();
            var tasks = new List<Task>();

            // discover the current infrastructure
            foreach (var instance in RoleEnvironment.Roles["WebClient"].Instances)
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

        private Task FlushQueue(object _)
        {
            this.Flush();
            return TaskDone.Done;
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

        public Task SendMessage(IReceivedMessage message)
        {
            var messagesToSend = Encoding.UTF8.GetString(message.GetBody());
            var msg = JsonHelper.DeserializeJsonToObject<MessageDto>(messagesToSend);

            foreach (var hub in hubs.Values)
            {
                try
                {
                    if (hub.Item1.State == ConnectionState.Connected)
                    {
                        hub.Item2.Invoke("MessageUpdate", messagesToSend, msg.DeviceId);
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

        private void Flush()
        {
            if (messageQueue.Count == 0) return;

            // send all messages to all SignalR hubs
            var messagesToSend = messageQueue.ToArray();
            messageQueue.Clear();
        }
    }
}
