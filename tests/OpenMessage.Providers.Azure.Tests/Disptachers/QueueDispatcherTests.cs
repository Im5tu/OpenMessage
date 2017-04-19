using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Providers.Azure.Configuration;
using OpenMessage.Providers.TestSpecifications;

namespace OpenMessage.Providers.Azure.Tests.Disptachers
{
    public class QueueDispatcherTests : DispatcherTests<string>
    {
        protected override string InterceptEntity => nameof(QueueDispatcherTests);

        protected override IServiceCollection ConfigureServices(IServiceCollection services)
            => services.AddOpenMessage().AddQueueDispatcher<string>().AddJsonNetSerializer().Configure<OpenMessageAzureProviderOptions<string>>(options => options.ConnectionString = "test");
    }
}
