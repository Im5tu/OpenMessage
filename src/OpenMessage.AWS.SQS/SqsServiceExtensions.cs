using Microsoft.Extensions.DependencyInjection;
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    public static class SqsServiceExtensions
    {
        public static ISqsConsumerBuilder<T> ConfigureSqsConsumer<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new SqsConsumerBuilder<T>(messagingBuilder);
        }

        public static ISqsDispatcherBuilder<T> ConfigureSqsDispatcher<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new SqsDispatcherBuilder<T>(messagingBuilder);
        }
    }
}