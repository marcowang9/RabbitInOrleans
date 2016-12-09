using Autofac;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansHost
{
    class DependencyRegistrationProvider : IBootstrapProvider
    {
        public DependencyRegistrationProvider(string name)
        {
            Name = name;
        }

        public DependencyRegistrationProvider()
        {
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            var containerBuilder = new ContainerBuilder();
            IocConfig.RegisterAllDepedancies(containerBuilder);
            containerBuilder.Build();
            return TaskDone.Done;
        }

        public string Name { get; private set; }
    }
}
