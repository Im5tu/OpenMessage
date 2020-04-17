using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenMessage.Apache.Kafka.HostedServices;
using OpenMessage.Builders;
using System;

namespace OpenMessage.Apache.Kafka.Configuration
{
    internal sealed class KafkaConsumerBuilder<TKey, TValue> : Builder, IKafkaConsumerBuilder<TKey, TValue>
    {
        private Action<HostBuilderContext, KafkaOptions>? _options;

        private string? _topicName = TypeCache<TValue>.FriendlyName?.ToLowerInvariant()
                                                     .Replace("<", "_")
                                                     .Replace(">", "_");

        public KafkaConsumerBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder) { }

        public override void Build()
        {
            var appName = HostBuilder.Context.HostingEnvironment.ApplicationName;
            HostBuilder.Services.AddTransient<IKafkaConsumer<TKey, TValue>, KafkaConsumer<TKey, TValue>>();
            HostBuilder.Services.AddConsumerService<KafkaMessagePump<TKey, TValue>>(ConsumerId);

            HostBuilder.Services.TryAddConsumerService<TValue>()
                       .TryAddSingleton<IPostConfigureOptions<KafkaOptions>, KafkaOptionsPostConfigurationProvider>();
            HostBuilder.TryConfigureDefaultPipeline<TValue>();

            ConfigureOptions<KafkaOptions>((cntx, o) =>
            {
                o.TopicName = _topicName;

                _options?.Invoke(cntx, o);
            });

            HostBuilder.Services.PostConfigure<KafkaOptions>(ConsumerId, options =>
            {
                if (!options.KafkaConfiguration.ContainsKey("group.id"))
                    options.KafkaConfiguration["group.id"] = appName;
            });
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
    }
}