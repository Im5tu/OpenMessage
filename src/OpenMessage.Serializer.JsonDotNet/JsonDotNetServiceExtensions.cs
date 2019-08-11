using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage.Serializer.JsonDotNet
{
    /// <summary>
    ///     Service Extensions
    /// </summary>
    public static class JsonDotNetServiceExtensions
    {
        /// <summary>
        ///     Adds both the JsonDotNet serializer &amp; deserializer
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <returns>The modified service collection</returns>
        public static void ConfigureJsonDotNet(this IMessagingBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureJsonDotNetDeserializer().ConfigureJsonDotNetSerializer();
        }

        /// <summary>
        ///     Adds the JsonDotNet deserializer only.
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNetDeserializer(this IMessagingBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSerialisation().AddDeserializer<JsonDotNetDeserializer>();
            return builder;
        }

        /// <summary>
        ///     Adds the JsonDotNet serializer only.
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNetSerializer(this IMessagingBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSerialisation().AddSerializer<JsonDotNetSerializer>();
            return builder;
        }
    }
}