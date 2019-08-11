using System;
using System.Text;
using Newtonsoft.Json;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetSerializer : ISerializer
    {
        public string ContentType => Constants.ContentType;

        public byte[] AsBytes<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity));
        }

        public string AsString<T>(T entity)
        {
            entity.Must(nameof(entity)).NotBeNull();
            return JsonConvert.SerializeObject(entity);
        }
    }
}
