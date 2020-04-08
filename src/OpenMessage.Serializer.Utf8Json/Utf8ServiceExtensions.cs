using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.Utf8Json
{
    /// <summary>
    ///     Utf8Json Service Extensions
    /// </summary>
    public static class Utf8JsonSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the Utf8Json serializer &amp; deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureUtf8Json(this IMessagingBuilder messagingBuilder) => messagingBuilder.ConfigureUtf8JsonDeserializer()
                                                                                                                      .ConfigureUtf8JsonSerializer();

        /// <summary>
        ///     Adds the Utf8Json deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureUtf8JsonDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<Utf8Serializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<IDeserializer>(sp => sp.GetRequiredService<Utf8Serializer>());

            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the Utf8Json serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureUtf8JsonSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<Utf8Serializer>();

            messagingBuilder.Services.AddSerialization()
                            .AddSingleton<ISerializer>(sp => sp.GetRequiredService<Utf8Serializer>());

            return messagingBuilder;
        }
    }
}