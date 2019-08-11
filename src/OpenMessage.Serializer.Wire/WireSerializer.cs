using System;
using System.Collections.Generic;
using System.IO;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.Wire
{
    internal sealed class WireSerializer : ISerializer, IDeserializer
    {
        private static readonly global::Wire.Serializer _serialiser = new global::Wire.Serializer();
        private static readonly string _contentType = "binary/wire";

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
            _serialiser.Serialize(entity, ms);
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
            return _serialiser.Deserialize<T>(ms);
        }
    }
}