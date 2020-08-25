using Microsoft.Extensions.DependencyInjection;
using OpenMessage.AWS.SNS.Configuration;

namespace OpenMessage.AWS.SNS
{
    /// <summary>
    ///     SNS Extensions
    /// </summary>
    public static class SnsServiceExtensions
    {
        /// <summary>
        ///     Returns an SNS dispatcher builder
        /// </summary>
        /// <param name="messagingBuilder">The host the dispatcher belongs to</param>
        /// <typeparam name="T">The type of message to dispatch</typeparam>
        /// <returns>An SNS dispatcher builder</returns>
        public static ISnsDispatcherBuilder<T> ConfigureSnsDispatcher<T>(this IMessagingBuilder messagingBuilder) => new SnsDispatcherBuilder<T>(messagingBuilder);
    }
}