using System.Collections.Generic;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;
using Utf8Json;

namespace OpenMessage.Serializer.Utf8Json
{
    internal sealed class Utf8Serializer : ISerializer, IDeserializer
    {
        private static readonly string _contentType = "application/json";

        public string ContentType { get; } = _contentType;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {_contentType};

        public string AsString<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return JsonSerializer.ToJsonString(entity);
        }

        public byte[] AsBytes<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();

            return JsonSerializer.Serialize(entity);
        }

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return JsonSerializer.Deserialize<T>(data);
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            return JsonSerializer.Deserialize<T>(data);
        }
    }
}