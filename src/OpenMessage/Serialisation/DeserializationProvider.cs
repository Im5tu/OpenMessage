using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenMessage.Serialisation
{
    internal sealed class DeserializationProvider : IDeserializationProvider
    {
        private readonly Dictionary<string, IDeserializer> _deserializers = new Dictionary<string, IDeserializer>(StringComparer.OrdinalIgnoreCase);

        public DeserializationProvider(IEnumerable<IDeserializer> deserializers)
        {
            if (deserializers is null)
                Throw.ArgumentNullException(nameof(deserializers));

            foreach (var deserializer in deserializers)
            foreach (var contentType in deserializer.SupportedContentTypes)
                _deserializers[contentType] = deserializer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T From<T>(string data, string contentType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            if (string.IsNullOrWhiteSpace(contentType))
                Throw.ArgumentException(nameof(contentType), "Cannot be null, empty or whitespace");

            return GetDeserializer(contentType)
                .From<T>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T From<T>(byte[] data, string contentType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            if (string.IsNullOrWhiteSpace(contentType))
                Throw.ArgumentException(nameof(contentType), "Cannot be null, empty or whitespace");

            return GetDeserializer(contentType)
                .From<T>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDeserializer GetDeserializer(string contentType)
        {
            if (!_deserializers.TryGetValue(contentType, out var deserializer))
                Throw.Exception($"No deserializer registered for content type: {deserializer}. Registered types: {string.Join(", ", _deserializers.Keys)}");

            return deserializer;
        }
    }
}