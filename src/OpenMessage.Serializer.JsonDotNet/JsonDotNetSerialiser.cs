using System.Text;
using Newtonsoft.Json;
using OpenMessage.Serialization;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetSerializer : ISerializer
    {
        public string ContentType => Constants.ContentType;

        public byte[] AsBytes<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity));
        }

        public string AsString<T>(T entity)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return JsonConvert.SerializeObject(entity);
        }
    }
}