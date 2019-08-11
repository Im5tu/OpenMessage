using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.Jil
{
    /// <summary>
    ///     Jil Service Extensions
    /// </summary>
    public static class JilSerializerServiceExtensions
    {
        /// <summary>
        ///     Adds the Jil serializer & deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureJil(this IMessagingBuilder messagingBuilder)
        {
            return messagingBuilder.ConfigureJilDeserializer().ConfigureJilSerializer();
        }

        /// <summary>
        ///     Adds the Jil serializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureJilSerializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<JilSerializer>();
            messagingBuilder.Services.AddSerialization().AddSingleton<ISerializer>(sp => sp.GetRequiredService<JilSerializer>());
            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the Jil deserializer
        /// </summary>
        /// <param name="messagingBuilder">The host to configure</param>
        /// <returns>The modified builder</returns>
        public static IMessagingBuilder ConfigureJilDeserializer(this IMessagingBuilder messagingBuilder)
        {
            messagingBuilder.Services.TryAddSingleton<JilSerializer>();
            messagingBuilder.Services.AddSerialization().AddSingleton<IDeserializer>(sp => sp.GetRequiredService<JilSerializer>());
            return messagingBuilder;
        }
    }
}