using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.MessagePack
{
    /// <summary>
    ///     MessagePack Service Extensions
    /// </summary>
    public static class MessagePackSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the MessagePack serializer &amp; deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureMessagePack(this IMessagingBuilder messagingBuilder) => messagingBuilder.ConfigureMessagePackDeserializer()
                                                                                                                         .ConfigureMessagePackSerializer();

        /// <summary>
        ///     Adds the MessagePack deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureMessagePackDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<MessagePackSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<IDeserializer>(sp => sp.GetRequiredService<MessagePackSerializer>());

            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the MessagePack serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureMessagePackSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<MessagePackSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<ISerializer>(sp => sp.GetRequiredService<MessagePackSerializer>());

            return messagingBuilder;
        }
    }
}