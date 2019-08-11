using System.Text.Json;
using OpenMessage.Extensions;

namespace OpenMessage.Serialisation
{
    internal sealed class DefaultSerializer : ISerializer
    {
        public string ContentType { get; } = "application/json";

        public byte[] AsBytes<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNullOrDefault();

            return JsonSerializer.SerializeToUtf8Bytes(entity);
        }

        public string AsString<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNullOrDefault();

            return JsonSerializer.Serialize(entity);
        }
    }
}