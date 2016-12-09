using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RabbitMQMessaging;
using System.Text;

namespace WebClient.Controllers
{
    public class SignalRHub : Hub
    {
        public void MessageUpdate(string message, string deviceId)
        {
            // Forward a single messages to all browsers
            //Clients.Group("BROWSERS").messageUpdate(message);
            Clients.Client(deviceId).messageUpdate(message);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            if (Context.Headers.Get("ORLEANS") != "GRAIN")
            {
                // This connection does not have the GRAIN header, so it must be a browser
                // Therefore add this connection to the browser group
                Groups.Add(Context.ConnectionId, "BROWSERS");
            }
            return base.OnConnected();
        }


    }
}