using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenMessage.Extensions;

namespace OpenMessage.Serialisation
{
    internal sealed class DeserializationProvider : IDeserializationProvider
    {
        private readonly Dictionary<string, IDeserializer> _deserializers = new Dictionary<string, IDeserializer>(StringComparer.OrdinalIgnoreCase);

        public DeserializationProvider(IEnumerable<IDeserializer> deserializers)
        {
            deserializers.Must(nameof(deserializers)).NotBeNull();

            foreach (var deserializer in deserializers)
            foreach (var contentType in deserializer.SupportedContentTypes)
                _deserializers[contentType] = deserializer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T From<T>(string data, string contentType)
        {
            data.Must(nameof(data)).NotBeNullOrWhiteSpace();
            contentType.Must(nameof(contentType)).NotBeNullOrWhiteSpace();

            return GetDeserializer(contentType).From<T>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T From<T>(byte[] data, string contentType)
        {
            data.Must(nameof(data)).NotBeNullOrEmpty();
            contentType.Must(nameof(contentType)).NotBeNullOrWhiteSpace();

            return GetDeserializer(contentType).From<T>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDeserializer GetDeserializer(string contentType)
        {
            if (!_deserializers.TryGetValue(contentType, out var deserializer)) Throw.Exception($"No deserializer registered for content type: {deserializer}. Registered types: {string.Join(", ", _deserializers.Keys)}");

            return deserializer;
        }
    }
}