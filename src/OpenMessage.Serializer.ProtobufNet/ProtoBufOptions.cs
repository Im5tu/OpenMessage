using ProtoBuf.Meta;

namespace OpenMessage.Serializer.ProtobufNet
{
    public class ProtoBufOptions
    {
        public TypeModel TypeModel { get; set; } = RuntimeTypeModel.Default;
    }
}
