using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.Protobuf
{
    /// <summary>
    ///     Protobuf Service Extensions
    /// </summary>
    public static class ProtobufSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the Protobuf serializer &amp; deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureProtobuf(this IMessagingBuilder messagingBuilder) => messagingBuilder.ConfigureProtobufDeserializer()
                                                                                                                      .ConfigureProtobufSerializer();

        /// <summary>
        ///     Adds the Protobuf deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureProtobufDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<ProtobufSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<IDeserializer>(sp => sp.GetRequiredService<ProtobufSerializer>());

            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the Protobuf serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureProtobufSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<ProtobufSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<ISerializer>(sp => sp.GetRequiredService<ProtobufSerializer>());

            return messagingBuilder;
        }
    }
}