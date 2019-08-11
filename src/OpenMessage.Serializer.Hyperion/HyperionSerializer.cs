using System;
using System.Collections.Generic;
using System.IO;
using Hyperion;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.Hyperion
{
    using Serializer = global::Hyperion.Serializer;
    // TODO :: Expose settings via options

    internal sealed class HyperionSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/hyperion";
        private static readonly Serializer _serialiser = new Serializer(new SerializerOptions(preserveObjectReferences: true));

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