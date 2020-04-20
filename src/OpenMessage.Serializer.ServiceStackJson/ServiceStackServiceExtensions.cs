using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.ServiceStackJson
{
    /// <summary>
    ///     ServiceStackJson Service Extensions
    /// </summary>
    public static class ServiceStackJsonSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the ServiceStackJson serializer &amp; deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureServiceStackJson(this IMessagingBuilder messagingBuilder) => messagingBuilder.ConfigureServiceStackJsonDeserializer()
                                                                                                                              .ConfigureServiceStackJsonSerializer();

        /// <summary>
        ///     Adds the ServiceStackJson deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureServiceStackJsonDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<ServiceStackSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<IDeserializer>(sp => sp.GetRequiredService<ServiceStackSerializer>());

            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the ServiceStackJson serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureServiceStackJsonSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<ServiceStackSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<ISerializer>(sp => sp.GetRequiredService<ServiceStackSerializer>());

            return messagingBuilder;
        }
    }
}