using System;
using OpenMessage.Serialization;
using ServiceStack.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenMessage.Serializer.ServiceStackJson
{
    internal sealed class ServiceStackSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "application/json";

        public string ContentType { get; } = _contentType;

        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return Encoding.UTF8.GetBytes(JsonSerializer.SerializeToString(entity));
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.SerializeToString(entity);
        }

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return (T) JsonSerializer.DeserializeFromString(data, messageType);
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            using var ms = new MemoryStream(data);

            return (T) JsonSerializer.DeserializeFromStream(messageType, ms);
        }
    }
}