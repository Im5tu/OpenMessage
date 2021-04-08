using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenMessage.Builders;
using System;

namespace OpenMessage.AWS.SQS.Configuration
{
    internal sealed class SqsConsumerBuilder<T> : Builder, ISqsConsumerBuilder<T> where T : class
    {
        private Action<HostBuilderContext, SQSConsumerOptions>? _configuration;

        public SqsConsumerBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder) { }

        public override void Build()
        {
            HostBuilder.Services.TryAddConsumerService<T>();
            HostBuilder.TryConfigureDefaultPipeline<T>();

            if (_configuration is {})
                ConfigureOptions(_configuration);

            HostBuilder.Services.TryAddTransient<ISqsConsumer<T>, SqsConsumer<T>>();
            HostBuilder.Services.TryAddTransient<IQueueMonitor<T>, QueueMonitor<T>>();
            HostBuilder.Services.AddConsumerService<SqsMessagePump<T>>(ConsumerId);
        }

        public ISqsConsumerBuilder<T> FromConfiguration(Action<SQSConsumerOptions> configuration)
        {
            return FromConfiguration((context, options) => configuration(options));
        }

        public ISqsConsumerBuilder<T> FromConfiguration(Action<HostBuilderContext, SQSConsumerOptions> configuration)
        {
            _configuration = configuration;

            return this;
        }

        public ISqsConsumerBuilder<T> FromConfiguration(string configurationSection)
        {
            _configuration = (context, options) => context.Configuration.Bind(configurationSection, options);

            return this;
        }
    }
}