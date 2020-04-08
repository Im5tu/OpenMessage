using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.Wire
{
    /// <summary>
    ///     Wire Service Extensions
    /// </summary>
    public static class WireSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the Wire serializer &amp; deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureWire(this IMessagingBuilder messagingBuilder) => messagingBuilder.ConfigureWireDeserializer()
                                                                                                                  .ConfigureWireSerializer();

        /// <summary>
        ///     Adds the Wire deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureWireDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<WireSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<IDeserializer>(sp => sp.GetRequiredService<WireSerializer>());

            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the Wire serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureWireSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<WireSerializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<ISerializer>(sp => sp.GetRequiredService<WireSerializer>());

            return messagingBuilder;
        }
    }
}