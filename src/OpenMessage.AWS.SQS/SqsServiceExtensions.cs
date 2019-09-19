using Microsoft.Extensions.DependencyInjection;
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    /// <summary>
    /// SQS Extensions
    /// </summary>
    public static class SqsServiceExtensions
    {
        /// <summary>
        /// Returns an SQS consumer builder
        /// </summary>
        /// <param name="messagingBuilder">The host the consumer belongs to</param>
        /// <typeparam name="T">The type of message to consume</typeparam>
        /// <returns>An SQS consumer builder</returns>
        public static ISqsConsumerBuilder<T> ConfigureSqsConsumer<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new SqsConsumerBuilder<T>(messagingBuilder);
        }

        /// <summary>
        /// Returns an SQS dispatcher builder
        /// </summary>
        /// <param name="messagingBuilder">The host the dispatcher belongs to</param>
        /// <typeparam name="T">The type of message to dispatch</typeparam>
        /// <returns>An SQS dispatcher builder</returns>
        public static ISqsDispatcherBuilder<T> ConfigureSqsDispatcher<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new SqsDispatcherBuilder<T>(messagingBuilder);
        }
    }
}