using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Channels;

namespace OpenMessage.Memory
{
    internal sealed class MemoryProviderBuilder<T> : IMemoryProviderBuilder<T>
    {
        private Func<IServiceProvider, Channel<Message<T>>>? _channelCreator;

        public IMessagingBuilder HostBuilder { get; }

        public MemoryProviderBuilder(IMessagingBuilder builder) => HostBuilder = builder;

        public void Build()
        {
            if (!(_channelCreator is null))
            {
                HostBuilder.Services.TryAddChannel(_channelCreator)
                    .TryAddConsumerService(_channelCreator)
                    .AddSingleton<IDispatcher<T>, MemoryDispatcher<T>>();
            }

            HostBuilder.TryConfigureDefaultPipeline<T>();
        }

        public IMemoryProviderBuilder<T> ConfigureChannel(Func<IServiceProvider, Channel<Message<T>>> channelCreator)
        {
            _channelCreator = channelCreator;

            return this;
        }
    }
}