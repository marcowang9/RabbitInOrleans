using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebClient.Startup))]
namespace WebClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}