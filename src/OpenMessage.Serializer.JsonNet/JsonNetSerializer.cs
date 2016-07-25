using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTelemetry;
using System;
using System.IO;
using System.Text;

namespace OpenMessage.Serializer.JsonNet
{
    internal sealed class JsonNetSerializer : ISerializer
    {
        private readonly ITelemetryContext _telemetry;
        private JsonSerializerSettings _settings;

        public string TypeName => "application/json";

        public JsonNetSerializer(ITelemetryContext telemetry, IOptions<JsonSerializerSettings> options)
        {
            if (telemetry == null)
                throw new ArgumentNullException(nameof(telemetry));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _telemetry = telemetry;
            _settings = options.Value;
        }

        public T Deserialize<T>(Stream entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var streamReader = new StreamReader(entity))
            using (_telemetry.RecordDuration($"{nameof(JsonNetSerializer)}.{nameof(Deserialize)}"))
                return JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd(), _settings);
        }

        public Stream Serializer<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (_telemetry.RecordDuration($"{nameof(JsonNetSerializer)}.{nameof(Deserialize)}"))
                return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity, _settings)));
        }
    }
}
