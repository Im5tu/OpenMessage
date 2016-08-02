using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace OpenMessage.Serializer.JsonNet
{
    internal sealed class JsonNetSerializer : ISerializer
    {
        private JsonSerializerSettings _settings;

        public string TypeName => "application/json";

        public JsonNetSerializer(IOptions<JsonSerializerSettings> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _settings = options.Value;
        }

        public T Deserialize<T>(Stream entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var streamReader = new StreamReader(entity))
                return JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd(), _settings);
        }

        public Stream Serialize<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity, _settings)));
        }
    }
}
