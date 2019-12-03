using OpenMessage.Serialisation;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenMessage.Serializer.Wire
{
    internal sealed class WireSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/wire";
        private static readonly global::Wire.Serializer _serialiser = new global::Wire.Serializer();

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            using var ms = new MemoryStream();
            _serialiser.Serialize(entity, ms);

            return ms.ToArray();
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return Convert.ToBase64String(AsBytes(entity));
        }

        public T From<T>(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return From<T>(Convert.FromBase64String(data));
        }

        public T From<T>(byte[] data)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            using var ms = new MemoryStream(data);

            return _serialiser.Deserialize<T>(ms);
        }
    }
}