using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Providers.Azure.Configuration;
using OpenMessage.Providers.TestSpecifications;

namespace OpenMessage.Providers.Azure.Tests.Observables
{
    public class QueueObservableTests : ObservableTests<string>
    {
        protected override IServiceCollection ConfigureServices(IServiceCollection services)
            => services.AddOpenMessage().AddQueueObservable<string>().Configure<OpenMessageAzureProviderOptions<string>>(options => options.ConnectionString = "test").AddJsonNetSerializer();
    }
}
