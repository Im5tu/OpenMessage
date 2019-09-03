using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Apache.Kafka.Configuration;
using OpenMessage.Apache.Kafka.HostedServices;
using OpenMessage.Configuration;

namespace OpenMessage.Apache.Kafka
{
    internal sealed class KafkaConsumerBuilder<TKey, TValue> : Builder, IKafkaConsumerBuilder<TKey, TValue>
    {
        private string _topicName = TypeCache<TValue>.FriendlyName.ToLowerInvariant().Replace("<", "_").Replace(">", "_");
        private Action<HostBuilderContext, KafkaOptions> _options;

        public KafkaConsumerBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder)
        {
        }

        public IKafkaConsumerBuilder<TKey, TValue> FromConfiguration(Action<KafkaOptions> configuration)
        {
            return FromConfiguration((context, options) => configuration(options));
        }

        public IKafkaConsumerBuilder<TKey, TValue> FromConfiguration(Action<HostBuilderContext, KafkaOptions> configuration)
        {
            _options = configuration;
            return this;
        }

        public IKafkaConsumerBuilder<TKey, TValue> FromTopic(string topicName)
        {
            if (!string.IsNullOrWhiteSpace(topicName))
                _topicName = topicName;

            return this;
        }

        public override void Build()
        {
            var appName = HostBuilder.Context.HostingEnvironment.ApplicationName;
            HostBuilder.Services.AddTransient<IKafkaConsumer<TKey, TValue>, KafkaConsumer<TKey, TValue>>();
            HostBuilder.Services.AddConsumerService<KafkaMessagePump<TKey, TValue>>(ConsumerId);
            ConfigureOptions<KafkaOptions>((cntx, o) =>
            {
                o.TopicName = _topicName;
                if (_options != null)
                    _options(cntx, o);
            });
            HostBuilder.Services.PostConfigure<KafkaOptions>(ConsumerId, options =>
            {
                if (!options.KafkaConfiguration.ContainsKey("group.id"))
                    options.KafkaConfiguration["group.id"] = appName;
            });
        }
    }
}