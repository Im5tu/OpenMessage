using System.Collections.Generic;
using System.Text.Json;

namespace OpenMessage.Serialisation
{
    internal sealed class DefaultDeserializer : IDeserializer
    {
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {"application/json"};

        public T From<T>(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            return JsonSerializer.Deserialize<T>(data);
        }

        public T From<T>(byte[] data)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            return JsonSerializer.Deserialize<T>(data);
        }
    }
}