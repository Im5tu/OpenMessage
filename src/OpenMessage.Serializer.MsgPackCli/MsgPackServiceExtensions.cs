using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.MsgPackCli
{
    /// <summary>
    ///     MsgPack Service Extensions
    /// </summary>
    public static class MsgPackSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the MsgPack serializer &amp; deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureMsgPack(this IMessagingBuilder messagingBuilder) => messagingBuilder.ConfigureMsgPackDeserializer()
                                                                                                                     .ConfigureMsgPackSerializer();

        /// <summary>
        ///     Adds the MsgPack deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureMsgPackDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<MsgPackSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<IDeserializer>(sp => sp.GetRequiredService<MsgPackSerializer>());

            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the MsgPack serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureMsgPackSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<MsgPackSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<ISerializer>(sp => sp.GetRequiredService<MsgPackSerializer>());

            return messagingBuilder;
        }
    }
}