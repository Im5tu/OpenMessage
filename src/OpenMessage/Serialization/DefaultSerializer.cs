using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OpenMessage.Serialization
{
    internal sealed class DefaultSerializer : ISerializer
    {
        private JsonSerializerOptions _settings;
        public string ContentType { get; } = "application/json";

        public DefaultSerializer(IOptionsMonitor<JsonSerializerOptions> settings)
        {
            _settings = settings.Get(SerializationConstants.SerializerSettings);
        }

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.SerializeToUtf8Bytes(entity, _settings);
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonSerializer.Serialize(entity, _settings);
        }
    }
}