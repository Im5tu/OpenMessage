using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;
using ServiceStack.Text;

namespace OpenMessage.Serializer.ServiceStackJson
{
    internal sealed class ServiceStackSerializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "application/json";

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public string AsString<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return JsonSerializer.SerializeToString(entity);
        }

        public byte[] AsBytes<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return Encoding.UTF8.GetBytes(JsonSerializer.SerializeToString(entity));
        }

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return JsonSerializer.DeserializeFromString<T>(data);
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            using var ms = new MemoryStream(data);
            return JsonSerializer.DeserializeFromStream<T>(ms);
        }
    }
}