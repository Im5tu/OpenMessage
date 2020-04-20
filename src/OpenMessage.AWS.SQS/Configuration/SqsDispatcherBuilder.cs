using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Builders;
using System;
using System.Threading.Channels;

namespace OpenMessage.AWS.SQS.Configuration
{
    internal sealed class SqsDispatcherBuilder<T> : Builder, ISqsDispatcherBuilder<T>
    {
        private Action<HostBuilderContext, SQSDispatcherOptions<T>>? _configuration;
        private bool _batchedDispatcher = true;

        public SqsDispatcherBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder) { }

        public override void Build()
        {
            if (_configuration is {})
                ConfigureOptions(_configuration, true);

            if (_batchedDispatcher)
            {
                HostBuilder.Services.AddHostedService<SqsDispatcherService>();
                HostBuilder.Services.TryAddChannel(sp => Channel.CreateUnbounded<SendSqsMessageCommand>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }));
                HostBuilder.Services.AddSingleton<IDispatcher<T>, SqsBatchedDispatcher<T>>();
            }
            else
            {
                HostBuilder.Services.AddSingleton<IDispatcher<T>, SqsDispatcher<T>>();
            }
        }

        public ISqsDispatcherBuilder<T> FromConfiguration(Action<SQSDispatcherOptions<T>> configuration)
        {
            return FromConfiguration((context, options) => configuration(options));
        }

        public ISqsDispatcherBuilder<T> FromConfiguration(Action<HostBuilderContext, SQSDispatcherOptions<T>> configuration)
        {
            _configuration = configuration;

            return this;
        }

        public ISqsDispatcherBuilder<T> FromConfiguration(string configurationSection)
        {
            _configuration = (context, options) => context.Configuration.Bind(configurationSection, options);

            return this;
        }

        public ISqsDispatcherBuilder<T> WithBatchedDispatcher(bool enabled = true)
        {
            _batchedDispatcher = enabled;
            return this;
        }
    }
}