using System.Collections.Generic;
using System.Text.Json;
using OpenMessage.Extensions;

namespace OpenMessage.Serialisation
{
    internal sealed class DefaultDeserializer : IDeserializer
    {
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {"application/json"};

        public T From<T>(string data)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();

            return JsonSerializer.Deserialize<T>(data);
        }

        public T From<T>(byte[] data)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();

            return JsonSerializer.Deserialize<T>(data);
        }
    }
}