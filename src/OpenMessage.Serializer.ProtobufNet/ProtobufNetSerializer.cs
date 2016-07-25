using Microsoft.Extensions.Options;
using OpenTelemetry;
using ProtoBuf.Meta;
using System;
using System.IO;

namespace OpenMessage.Serializer.ProtobufNet
{
    public class ProtobufNetSerializer : ISerializer
    {
        private readonly TypeModel _model;
        private readonly ITelemetryContext _telemetry;

        public string TypeName => "application/protobuf";

        public ProtobufNetSerializer(ITelemetryContext telemetry, IOptions<ProtoBufOptions> options)
        {
            if (telemetry == null)
                throw new ArgumentNullException(nameof(telemetry));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _telemetry = telemetry;
            _model = options.Value.TypeModel;
        }

        public T Deserialize<T>(Stream entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (_telemetry.RecordDuration($"{nameof(ProtobufNetSerializer)}.{nameof(Deserialize)}"))
                return (T)_model.Deserialize(entity, null, typeof(T));
        }

        public Stream Serializer<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = new MemoryStream();
            using (_telemetry.RecordDuration($"{nameof(ProtobufNetSerializer)}.{nameof(Deserialize)}"))
            {
                _model.Serialize(result, entity);
                return result;
            }
        }
    }
}
