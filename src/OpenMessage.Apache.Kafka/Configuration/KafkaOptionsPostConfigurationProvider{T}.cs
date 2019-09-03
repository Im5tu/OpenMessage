using Microsoft.Extensions.Options;

namespace OpenMessage.Apache.Kafka.Configuration
{
    internal sealed class KafkaOptionsPostConfigurationProvider<T> : KafkaOptionsPostConfigurationProvider, IPostConfigureOptions<KafkaOptions<T>>
    {
        public void PostConfigure(string name, KafkaOptions<T> options)
        {
            base.PostConfigure(name, options);

            if (string.IsNullOrWhiteSpace(options.TopicName))
                options.TopicName = TypeCache<T>.FriendlyName.ToLowerInvariant().Replace("<", "_").Replace(">", "_");
        }
    }
}