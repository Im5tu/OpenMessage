using Jil;
using OpenTelemetry;
using System;
using System.IO;
using System.Text;
using JilOptions = Jil.Options;

namespace OpenMessage.Serializer.Jil
{
    public class JilSerializer : ISerializer
    {
        private readonly JilOptions _settings;
        private readonly ITelemetryContext _telemetry;

        public string TypeName => "application/json";

        public JilSerializer(ITelemetryContext telemetry, JilOptions options)
        {
            if (telemetry == null)
                throw new ArgumentNullException(nameof(telemetry));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _telemetry = telemetry;
            _settings = options;
        }

        public T Deserialize<T>(Stream entity)
        {
            using (var streamReader = new StreamReader(entity))
            using (_telemetry.RecordDuration($"{nameof(JilSerializer)}.{nameof(Deserialize)}"))
                return JSON.Deserialize<T>(streamReader.ReadToEnd(), _settings);
        }

        public Stream Serializer<T>(T entity)
        {
            using (_telemetry.RecordDuration($"{nameof(JilSerializer)}.{nameof(Deserialize)}"))
                return new MemoryStream(Encoding.UTF8.GetBytes(JSON.Serialize(entity, _settings)));
        }
    }
}
