using Microsoft.Extensions.DependencyInjection;
using System;

namespace OpenMessage.Providers.TestSpecifications
{
    public abstract class TestBase : IDisposable
    {
        protected IServiceProvider Services { get; set; }

        public TestBase()
        {
            var services = new ServiceCollection()
                                    .AddLogging()
                                    .AddOptions();
            Services = ConfigureServices(services).BuildServiceProvider();
        }

        protected abstract IServiceCollection ConfigureServices(IServiceCollection services);

        public virtual void Dispose()
        {
        }
    }
}
