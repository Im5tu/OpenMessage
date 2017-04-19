using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Providers.Azure.Configuration;
using OpenMessage.Providers.TestSpecifications;

namespace OpenMessage.Providers.Azure.Tests.Disptachers
{
    public class TopicDispatcherTests : DispatcherTests<string>
    {
        protected override string InterceptEntity => nameof(TopicDispatcherTests);

        protected override IServiceCollection ConfigureServices(IServiceCollection services)
            => services.AddOpenMessage().AddTopicDispatcher<string>().AddJsonNetSerializer().Configure<OpenMessageAzureProviderOptions<string>>(options => options.ConnectionString = "test");
    }
}
