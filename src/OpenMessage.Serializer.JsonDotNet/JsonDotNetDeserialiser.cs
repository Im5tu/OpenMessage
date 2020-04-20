using System;
using Newtonsoft.Json;
using OpenMessage.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetDeserializer : IDeserializer
    {
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {Constants.ContentType};

        public T From<T>(string data, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            var response = JsonConvert.DeserializeObject(data, messageType);
            if (response is null)
                Throw.Exception("Deserialization returned a null response");

            return (T)response;
        }

        public T From<T>(byte[] data, Type messageType)
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            var response = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), messageType);
            if (response is null)
                Throw.Exception("Deserialization returned a null response");

            return (T)response;
        }
    }
}