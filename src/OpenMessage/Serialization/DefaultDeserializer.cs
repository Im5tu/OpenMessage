using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenMessage.Serialization
{
    internal sealed class DefaultDeserializer : IDeserializer
    {
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {"application/json"};

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return (T)JsonSerializer.Deserialize(data, messageType);
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            return (T)JsonSerializer.Deserialize(data, messageType);
        }
    }
}