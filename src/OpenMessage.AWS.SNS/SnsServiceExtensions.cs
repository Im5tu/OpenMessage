using Microsoft.Extensions.DependencyInjection;
using OpenMessage.AWS.SNS.Configuration;

namespace OpenMessage.AWS.SNS
{
    public static class SnsServiceExtensions
    {
        public static ISnsDispatcherBuilder<T> ConfigureSqsDispatcher<T>(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddConsumerService<T>();
            return new SnsDispatcherBuilder<T>(messagingBuilder);
        }
    }
}