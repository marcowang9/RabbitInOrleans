using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.Text;

namespace WebHub.Controllers
{
    public class SignalRHub : Hub
    {
        public void MessageUpdate(string message)
        {
            // Forward a single messages to all browsers
            Clients.Group("BROWSERS").messageUpdate(message);
        }

        public void DeviceUpdate(string deviceId)
        {
            Clients.Group("BROWSERS").deviceUpdate(deviceId);
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