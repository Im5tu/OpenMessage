using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Configuration;

namespace OpenMessage.Memory
{
    internal sealed class MemoryDispatcherBuilder<T> : Builder, IMemoryDispatcherBuilder<T>
    {
        public MemoryDispatcherBuilder(IMessagingBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override void Build()
        {
            HostBuilder.Services.AddSingleton<IDispatcher<T>, MemoryDispatcher<T>>();
        }
    }
}