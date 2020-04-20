using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetSerializer : ISerializer
    {
        private JsonSerializerSettings _settings;
        public string ContentType => Constants.ContentType;

        public JsonDotNetSerializer(IOptionsMonitor<JsonSerializerSettings> settings)
        {
            _settings = settings.Get(SerializationConstants.SerializerSettings);
        }

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity, _settings));
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonConvert.SerializeObject(entity, _settings);
        }
    }
}