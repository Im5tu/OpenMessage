using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Configuration;

namespace OpenMessage.Memory
{
    internal sealed class MemoryConsumerBuilder<T> : Builder, IMemoryConsumerBuilder<T>
    {
        public MemoryConsumerBuilder(IMessagingBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override void Build()
        {
            HostBuilder.Services.TryAddConsumerService<T>();
        }
    }
}