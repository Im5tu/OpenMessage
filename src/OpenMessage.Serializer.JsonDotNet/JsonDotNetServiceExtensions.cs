using Microsoft.Extensions.DependencyInjection;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenMessage.Serialization;

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
        /// <param name="serializerConfigurator">Configure the serializer options</param>
        /// <param name="deserializerConfigurator">Configure the deserializer options</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNet(this IMessagingBuilder builder, Action<JsonSerializerSettings>? serializerConfigurator = null, Action<JsonSerializerSettings>? deserializerConfigurator = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureJsonDotNetDeserializer(deserializerConfigurator)
                          .ConfigureJsonDotNetSerializer(serializerConfigurator);
        }

        /// <summary>
        ///     Sets the specified settings to a snake_case_naming_strategy
        /// </summary>
        public static JsonSerializerSettings WithSnakeCaseNamingStrategy(this JsonSerializerSettings settings)
        {
            settings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            return settings;
        }

        /// <summary>
        ///     Adds the JsonDotNet deserializer only.
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <param name="configurator">Configure the deserializer options</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNetDeserializer(this IMessagingBuilder builder, Action<JsonSerializerSettings>? configurator = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSerialization()
                   .AddDeserializer<JsonDotNetDeserializer>()
                   .Configure<JsonSerializerSettings>(SerializationConstants.DeserializerSettings, settings =>
                   {
                       settings.NullValueHandling = NullValueHandling.Ignore;
                       configurator?.Invoke(settings);
                   });

            return builder;
        }

        /// <summary>
        ///     Adds the JsonDotNet serializer only.
        /// </summary>
        /// <param name="builder">The service collection to modify</param>
        /// <param name="configurator">Configure the serializer options</param>
        /// <returns>The modified service collection</returns>
        public static IMessagingBuilder ConfigureJsonDotNetSerializer(this IMessagingBuilder builder, Action<JsonSerializerSettings>? configurator = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSerialization()
                   .AddSerializer<JsonDotNetSerializer>()
                   .Configure<JsonSerializerSettings>(SerializationConstants.SerializerSettings, settings =>
                   {
                       settings.NullValueHandling = NullValueHandling.Ignore;
                       configurator?.Invoke(settings);
                   });;

            return builder;
        }
    }
}