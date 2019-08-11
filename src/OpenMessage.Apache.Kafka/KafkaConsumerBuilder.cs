using System;
using Microsoft.Extensions.Hosting;
using OpenMessage;
using OpenMessage.Apache.Kafka;
using OpenMessage.Apache.Kafka.Configuration;
using OpenMessage.Apache.Kafka.HostedServices;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class KafkaConsumerBuilder<TKey, TValue> : IKafkaConsumerBuilder<TKey, TValue>
    {
        private readonly IMessagingBuilder _messagingBuilder;
        private readonly string _consumerId = Guid.NewGuid().ToString("N");

        private string _topicName = TypeCache<TValue>.FriendlyName.ToLowerInvariant().Replace("<", ".").Replace(">", ".");
        private Action<HostBuilderContext, KafkaOptions> _options;

        public KafkaConsumerBuilder(IMessagingBuilder messagingBuilder)
        {
            _messagingBuilder = messagingBuilder;
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

        public void BuildConsumer()
        {
            var appName = _messagingBuilder.Context.HostingEnvironment.ApplicationName;
            _messagingBuilder.Services.AddTransient<IKafkaConsumer<TKey, TValue>, KafkaConsumer<TKey, TValue>>();
            _messagingBuilder.Services.AddSingleton<IHostedService>(sp => ActivatorUtilities.CreateInstance<KafkaMessagePump<TKey, TValue>>(sp, _consumerId));
            _messagingBuilder.Services.Configure<KafkaOptions>(_consumerId, options =>
            {
                options.TopicName = _topicName;
                _options?.Invoke(_messagingBuilder.Context, options);
            });
            _messagingBuilder.Services.PostConfigure<KafkaOptions>(_consumerId, options =>
            {
                if (!options.KafkaConfiguration.ContainsKey("group.id"))
                    options.KafkaConfiguration["group.id"] = appName;
            });
        }
    }
}