using System;
using System.Collections.Generic;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;
using serialiser = MessagePack.MessagePackSerializer;

namespace OpenMessage.Serializer.MessagePack
{
    internal sealed class MessagePackSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/messagepack";

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public string AsString<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return Convert.ToBase64String(AsBytes(entity));
        }

        public byte[] AsBytes<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return serialiser.Serialize(entity);
        }

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return From<T>(Convert.FromBase64String(data));
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            return serialiser.Deserialize<T>(data);
        }
    }
}