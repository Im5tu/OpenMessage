using System;
using Newtonsoft.Json;
using OpenMessage.Serialization;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace OpenMessage.Serializer.JsonDotNet
{
    internal sealed class JsonDotNetDeserializer : IDeserializer
    {
        private JsonSerializerSettings _settings;
        public IEnumerable<string> SupportedContentTypes { get; } = new[] {Constants.ContentType};

        public JsonDotNetDeserializer(IOptionsMonitor<JsonSerializerSettings> settings)
        {
            _settings = settings.Get(SerializationConstants.DeserializerSettings);
        }

        public T? From<T>(string data, Type messageType) where T : class
        {
            if (string.IsNullOrWhiteSpace(data))
                Throw.ArgumentException(nameof(data), "Cannot be null, empty or whitespace");

            var response = JsonConvert.DeserializeObject(data, messageType, _settings);
            if (response is null)
                Throw.Exception("Deserialization returned a null response");

            return (T?)response;
        }

        public T? From<T>(byte[] data, Type messageType) where T : class
        {
            if (data is null || data.Length == 0)
                Throw.ArgumentException(nameof(data), "Cannot be null or empty");

            var response = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), messageType, _settings);
            if (response is null)
                Throw.Exception("Deserialization returned a null response");

            return (T?)response;
        }
    }
}