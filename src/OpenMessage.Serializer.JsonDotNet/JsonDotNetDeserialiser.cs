using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetDeserializer : IDeserializer
    {
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {Constants.ContentType};

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();
            return JsonConvert.DeserializeObject<T>(data);
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }
    }
}