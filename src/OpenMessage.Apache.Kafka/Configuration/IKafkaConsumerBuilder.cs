using Microsoft.Extensions.Hosting;
using OpenMessage.Builders;
using System;

namespace OpenMessage.Apache.Kafka.Configuration
{
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IKafkaConsumerBuilder<TKey, TValue> : IBuilder
    {
        /// <summary>
        ///     Configures the consumer with the specified options
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <returns>The modified consumer builder</returns>
        IKafkaConsumerBuilder<TKey, TValue> FromConfiguration(Action<HostBuilderContext, KafkaOptions> configuration);

        /// <summary>
        ///     Configures the consumer with the specified options
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <returns>The modified consumer builder</returns>
        IKafkaConsumerBuilder<TKey, TValue> FromConfiguration(Action<KafkaOptions> configuration);

        /// <summary>
        ///     Configures the consumer to consume from the specified topic
        /// </summary>
        /// <param name="topicName">The name of the topic to consume from</param>
        /// <returns>The modified consumer builder</returns>
        IKafkaConsumerBuilder<TKey, TValue> FromTopic(string topicName);
    }
}