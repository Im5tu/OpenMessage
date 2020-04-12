using System.Text.Json;

namespace OpenMessage.Serialization
{
    internal sealed class DefaultSerializer : ISerializer
    {
        public string ContentType { get; } = "application/json";

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.SerializeToUtf8Bytes(entity);
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.Serialize(entity);
        }
    }
}