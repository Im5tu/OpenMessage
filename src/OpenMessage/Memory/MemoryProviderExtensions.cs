using OpenMessage.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for adding an InMemory Dispatcher/Consumer
    /// </summary>
    public static class MemoryProviderExtensions
    {
        /// <summary>
        /// Adds an InMemory consumer
        /// </summary>
        /// <typeparam name="T">The type of the message to consumer</typeparam>
        /// <param name="messagingBuilder"></param>
        /// <returns></returns>
        public static IMemoryConsumerBuilder<T> ConfigureMemoryConsumer<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new MemoryConsumerBuilder<T>(messagingBuilder);
        }

        /// <summary>
        ///     Adds an InMemory dispatcher
        /// </summary>
        /// <typeparam name="T">The type of the message to dispatch</typeparam>
        /// <param name="messagingBuilder"></param>
        /// <returns></returns>
        public static IMemoryDispatcherBuilder<T> ConfigureMemoryDispatcher<T>(this IMessagingBuilder messagingBuilder)
        {
            return new MemoryDispatcherBuilder<T>(messagingBuilder);
        }
    }
}