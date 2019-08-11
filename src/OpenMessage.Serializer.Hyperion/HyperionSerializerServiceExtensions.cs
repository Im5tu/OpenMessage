using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.Hyperion
{
    /// <summary>
    /// Hyperion Service Extensions
    /// </summary>
    public static class HyperionSerializerServiceExtensions
    {
        /// <summary>
        /// Adds the hyperion serializer & deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureHyperion(this IMessagingBuilder messagingBuilder)
        {
            return messagingBuilder.ConfigureHyperionDeserializer().ConfigureHyperionSerializer();
        }

        /// <summary>
        /// Adds the hyperion serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureHyperionSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<HyperionSerializer>();
            messagingBuilder.Services.AddSingleton<ISerializer>(sp => sp.GetRequiredService<HyperionSerializer>());
            return messagingBuilder;
        }

        /// <summary>
        /// Adds the hyperion deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureHyperionDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<HyperionSerializer>();
            messagingBuilder.Services.AddSingleton<IDeserializer>(sp => sp.GetRequiredService<HyperionSerializer>());
            return messagingBuilder;
        }
    }
}