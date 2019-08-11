using System.Collections.Generic;
using System.Text;
using Jil;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.Jil
{
    internal sealed class JilSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "application/json";

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public string AsString<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return JSON.Serialize(entity);
        }

        public byte[] AsBytes<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return Encoding.UTF8.GetBytes(JSON.Serialize(entity));
        }

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return JSON.Deserialize<T>(data);
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            return JSON.Deserialize<T>(Encoding.UTF8.GetString(data));
        }
    }
}