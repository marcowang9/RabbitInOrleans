using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebHub.Startup))]
namespace WebHub
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}