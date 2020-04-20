using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OpenMessage.Serialization
{
    internal sealed class DefaultDeserializer : IDeserializer
    {
        private JsonSerializerOptions _settings;

        public IEnumerable<string> SupportedContentTypes { get; } = new[] {"application/json"};

        public DefaultDeserializer(IOptionsMonitor<JsonSerializerOptions> settings)
        {
            _settings = settings.Get(SerializationConstants.DeserializerSettings);
        }

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return (T)JsonSerializer.Deserialize(data, messageType, _settings);
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            return (T)JsonSerializer.Deserialize(data, messageType, _settings);
        }
    }
}