using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SQS.Configuration
{
    internal sealed class SqsConsumerBuilder<T> : Builder, ISqsConsumerBuilder<T>
    {
        private Action<HostBuilderContext, SQSConsumerOptions> _configuration;
        private int _consumerCount = 1;

        public SqsConsumerBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder)
        {
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

        public ISqsConsumerBuilder<T> FromConsumerCount(int count)
        {
            if (count < 1 || count > 50)
                throw new ArgumentOutOfRangeException("Consumer count must be between 1 & 50.");

            _consumerCount = count;
            return this;
        }

        public ISqsConsumerBuilder<T> FromConfiguration(string configurationSection)
        {
            _configuration = (context, options) => context.Configuration.Bind(configurationSection, options);
            return this;
        }

        public override void Build()
        {
            ConfigureOptions(_configuration);

            for (var i = 0; i < _consumerCount; i++)
                CreateConsumer();
        }

        private void CreateConsumer()
        {
            HostBuilder.Services.TryAddTransient<ISqsConsumer<T>, SqsConsumer<T>>();
            HostBuilder.Services.AddConsumerService<SqsMessagePump<T>>(ConsumerId);
        }
    }
}