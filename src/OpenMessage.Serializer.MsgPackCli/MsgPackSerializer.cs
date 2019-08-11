using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MsgPack.Serialization;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.MsgPackCli
{
    internal sealed class MsgPackSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "binary/msgpack";
        private readonly ConcurrentDictionary<Type, MessagePackSerializer> _serialisers = new ConcurrentDictionary<Type, MessagePackSerializer>();

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

            return _serialisers.GetOrAdd(typeof(T), key => MessagePackSerializer.Get(key)).PackSingleObject(entity);
        }

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return From<T>(Convert.FromBase64String(data));
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            return (T) _serialisers.GetOrAdd(typeof(T), key => MessagePackSerializer.Get(key)).UnpackSingleObject(data);
        }
    }
}