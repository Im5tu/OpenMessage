using Microsoft.Extensions.DependencyInjection;
using System;

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
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.ConfigureJsonDotNetDeserializer()
                   .ConfigureJsonDotNetSerializer();
        }

        /// <summary>
        ///     Adds the JsonDotNet deserializer only.
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNetDeserializer(this IMessagingBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSerialization()
                   .AddDeserializer<JsonDotNetDeserializer>();

            return builder;
        }

        /// <summary>
        ///     Adds the JsonDotNet serializer only.
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNetSerializer(this IMessagingBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSerialization()
                   .AddSerializer<JsonDotNetSerializer>();

            return builder;
        }
    }
}