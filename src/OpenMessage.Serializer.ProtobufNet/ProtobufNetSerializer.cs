using Microsoft.Extensions.Options;
using ProtoBuf.Meta;
using System;
using System.IO;

namespace OpenMessage.Serializer.ProtobufNet
{
    public class ProtobufNetSerializer : ISerializer
    {
        private readonly TypeModel _model;

        public string TypeName => "application/protobuf";

        public ProtobufNetSerializer(IOptions<ProtoBufOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _model = options.Value.TypeModel;
        }

        public T Deserialize<T>(Stream entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return (T)_model.Deserialize(entity, null, typeof(T));
        }

        public Stream Serialize<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = new MemoryStream();
            _model.Serialize(result, entity);
            return result;
        }
    }
}
