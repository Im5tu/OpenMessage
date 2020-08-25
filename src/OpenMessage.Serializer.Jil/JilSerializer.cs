using System;
using Jil;
using OpenMessage.Serialization;
using System.Collections.Generic;
using System.Text;

namespace OpenMessage.Serializer.Jil
{
    internal sealed class JilSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "application/json";

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return Encoding.UTF8.GetBytes(JSON.Serialize(entity));
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JSON.Serialize(entity);
        }

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return (T)JSON.Deserialize(data, messageType);
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            return (T)JSON.Deserialize(Encoding.UTF8.GetString(data), messageType);
        }
    }
}