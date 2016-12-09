using Common;
using Orleans;
using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebHub
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if (AzureEnvironment.IsInAzure)
            {
                // running in Azure
                if (AzureClient.IsInitialized == false)
                {
                    var clientConfigurationFile = AzureConfigUtils.ClientConfigFileLocation;
                    if (clientConfigurationFile.Exists == false)
                    {
                        throw new FileNotFoundException(string.Format("Cannot find orleans config file for initialization at {0}", clientConfigurationFile.FullName), clientConfigurationFile.FullName);
                    }
                    AzureClient.Initialize(clientConfigurationFile);
                }
            }
            else
            {
                // not running in Azure
                GrainClient.Initialize(Server.MapPath("/ClientOrleansConfiguration.xml"));

            }
        }
    }
}
