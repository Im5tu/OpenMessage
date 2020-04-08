using MsgPack.Serialization;
using OpenMessage.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenMessage.Serializer.MsgPackCli
{
    internal sealed class MsgPackSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/msgpack";
        private readonly ConcurrentDictionary<Type, MessagePackSerializer> _serialisers = new ConcurrentDictionary<Type, MessagePackSerializer>();

        public string ContentType { get; } = _contentType;

        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return _serialisers.GetOrAdd(typeof(T), key => MessagePackSerializer.Get(key))
                               .PackSingleObject(entity);
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

            return (T) _serialisers.GetOrAdd(messageType, key => MessagePackSerializer.Get(key))
                                   .UnpackSingleObject(data);
        }
    }
}