using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Orleans.Providers;
using Orleans;

namespace OrleansWorker
{
    public abstract class CompositeBootstrapProviderBase : IBootstrapProvider
    {
        private readonly ReadOnlyCollection<IBootstrapProvider> _subProviders;
        private string _name;

        protected CompositeBootstrapProviderBase(params IBootstrapProvider[] subProviders)
        {
            _subProviders = new ReadOnlyCollection<IBootstrapProvider>(subProviders);
        }

        public string Name
        {
            get { return _name; }
        }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            _name = name;
            var logger = providerRuntime.GetLogger("OrleansLog");
            const int InfoCode = 1000;
            const int ErrorCode = 1002;

            foreach (var subProvider in _subProviders)
            {
                var subProviderName = subProvider.Name;
                subProviderName = subProviderName ?? string.Format("subProvider-{0}_{1}", _name, Guid.NewGuid());
                logger.Info(InfoCode, "Info", string.Format("subProviderName is {0}", subProviderName), null, null);
                try
                {
                    await subProvider.Init(subProviderName, providerRuntime, config);
                }
                catch (Exception e)
                {
                    logger.Error(ErrorCode, string.Format("Failed to call Init method of provider - {0}, exception: ", subProviderName), null);
                    throw;
                }
            }
        }
    }
}
