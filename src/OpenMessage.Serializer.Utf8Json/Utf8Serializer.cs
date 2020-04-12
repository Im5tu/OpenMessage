using System;
using OpenMessage.Serialization;
using System.Collections.Generic;
using Utf8Json;

namespace OpenMessage.Serializer.Utf8Json
{
    internal sealed class Utf8Serializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "application/json";

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.Serialize(entity);
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.ToJsonString(entity);
        }

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return (T) JsonSerializer.NonGeneric.Deserialize(messageType, data);
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            return (T) JsonSerializer.NonGeneric.Deserialize(messageType, data);
        }
    }
}