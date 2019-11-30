using System;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage.Memory
{
    internal class MemoryProviderBuilder<T> : IMemoryProviderBuilder<T>
    {
        private Func<IServiceProvider, Channel<Message<T>>> _channelCreator;
        private Action<MemoryOptions<T>> _options;
        private readonly IMessagingBuilder _hostBuilder;

        public MemoryProviderBuilder(IMessagingBuilder builder)
        {
            _hostBuilder = builder;
        }

        public IMemoryProviderBuilder<T> ConfigureOptions(Action<MemoryOptions<T>> options)
        {
            _options = options;
            return this;
        }

        public IMemoryProviderBuilder<T> ConfigureChannel(Func<IServiceProvider, Channel<Message<T>>> channelCreator)
        {
            _channelCreator = channelCreator;
            return this;
        }

        public void Build()
        {
            _hostBuilder
                .Services
                .TryAddChannel(_channelCreator)
                .TryAddConsumerService(_channelCreator)
                .AddSingleton<IDispatcher<T>, MemoryDispatcher<T>>()
                .AddOptions<MemoryOptions<T>>();

            if (_options != null)
            {
                _hostBuilder.Services.PostConfigure(_options);
            }
        }
    }
}