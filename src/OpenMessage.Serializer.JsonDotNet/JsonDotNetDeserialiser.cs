using Newtonsoft.Json;
using OpenMessage.Serialisation;
using System.Collections.Generic;
using System.Text;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetDeserializer : IDeserializer
    {
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {Constants.ContentType};

        public T From<T>(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return JsonConvert.DeserializeObject<T>(data);
        }

        public T From<T>(byte[] data)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }
    }
}