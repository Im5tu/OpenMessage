using System;
using System.Collections.Generic;
using System.IO;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.Protobuf
{
    using Serializer = Protobuf;

    internal sealed class ProtobufSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/protobuf";

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

            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, entity);
            return ms.ToArray();
        }

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return From<T>(Convert.FromBase64String(data));
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            using var ms = new MemoryStream(data);
            return ProtoBuf.Serializer.Deserialize<T>(ms);
        }
    }
}