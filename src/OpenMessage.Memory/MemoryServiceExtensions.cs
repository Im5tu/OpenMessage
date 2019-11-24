using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage.Memory
{
    public static class MemoryServiceExtensions
    {
        public static IMemoryConsumerBuilder<T> ConfigureMemoryConsumer<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new MemoryConsumerBuilder<T>(messagingBuilder);
        }

        public static IMemoryDispatcherBuilder<T> ConfigureMemoryDispatcher<T>(this IMessagingBuilder messagingBuilder)
        {
            return new MemoryDispatcherBuilder<T>(messagingBuilder);
        }
    }
}