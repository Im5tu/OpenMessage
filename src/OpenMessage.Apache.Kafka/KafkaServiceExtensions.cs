using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenMessage;
using OpenMessage.Apache.Kafka;
using OpenMessage.Apache.Kafka.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for adding a Kafka Consumer or Dispatcher
    /// </summary>
    public static class KafkaServiceExtensions
    {
        /// <summary>
        /// Adds a kafka consumer
        /// </summary>
        /// <param name="messagingBuilder">The host builder</param>
        /// <typeparam name="T">The type to consume</typeparam>
        /// <returns>The modified builder</returns>
        public static IKafkaConsumerBuilder<string, T> ConfigureKafkaConsumer<T>(this IMessagingBuilder messagingBuilder)
        {
            return messagingBuilder.ConfigureKafkaConsumer<string, T>();
        }

        /// <summary>
        /// Adds a kafka consumer
        /// </summary>
        /// <param name="messagingBuilder">The host builder</param>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the message</typeparam>
        /// <returns>The modified builder</returns>
        public static IKafkaConsumerBuilder<TKey, TValue> ConfigureKafkaConsumer<TKey, TValue>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<TValue>().TryAddSingleton<IPostConfigureOptions<KafkaOptions>, KafkaOptionsPostConfigurationProvider>();
            return new KafkaConsumerBuilder<TKey, TValue>(messagingBuilder);
        }

        /// <summary>
        /// Adds a kafka dispatcher
        /// </summary>
        /// <param name="messagingBuilder">The host builder</param>
        /// <param name="options">Options for the dispatcher</param>
        /// <typeparam name="T">The type to dispatch</typeparam>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureKafkaDispatcher<T>(this IMessagingBuilder messagingBuilder, Action<KafkaOptions<T>> options = null)
        {
            if (options != null)
                messagingBuilder.Services.Configure(options);

            messagingBuilder.Services.AddSingleton<IDispatcher<T>, KafkaDispatcher<T>>().TryAddSingleton<IPostConfigureOptions<KafkaOptions<T>>, KafkaOptionsPostConfigurationProvider<T>>();

            return messagingBuilder;
        }

        /// <summary>
        /// Adds a kafka dispatcher
        /// </summary>
        /// <param name="messagingBuilder">The host builder</param>
        /// <param name="options">Options for the dispatcher</param>
        /// <typeparam name="T">The type to dispatch</typeparam>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureKafkaDispatcher<T>(this IMessagingBuilder messagingBuilder, Action<HostBuilderContext, KafkaOptions<T>> options = null)
        {
            if (options != null)
                messagingBuilder.Services.Configure<KafkaOptions<T>>(o => options(messagingBuilder.Context, o));

            messagingBuilder.Services.AddSingleton<IDispatcher<T>, KafkaDispatcher<T>>().TryAddSingleton<IPostConfigureOptions<KafkaOptions<T>>, KafkaOptionsPostConfigurationProvider<T>>();

            return messagingBuilder;
        }
    }
}