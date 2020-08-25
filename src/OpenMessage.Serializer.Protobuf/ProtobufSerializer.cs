using OpenMessage.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenMessage.Serializer.Protobuf
{
    internal sealed class ProtobufSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/protobuf";

        public string ContentType { get; } = _contentType;

        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, entity);

            return ms.ToArray();
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return Convert.ToBase64String(AsBytes(entity));
        }

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return From<T>(Convert.FromBase64String(data), messageType);
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            using var ms = new MemoryStream(data);

            return (T)ProtoBuf.Serializer.Deserialize(messageType, ms);
        }
    }
}