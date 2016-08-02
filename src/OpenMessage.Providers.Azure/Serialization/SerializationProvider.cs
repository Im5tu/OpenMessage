using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenMessage.Providers.Azure.Serialization
{
    internal sealed class SerializationProvider : ISerializationProvider
    {
        private readonly ISerializer _defaultSerializer;
        private readonly ISerializer[] _providers;

        public SerializationProvider(IEnumerable<ISerializer> providers,
            ISerializer defaultSerializer)
        {
            if (providers == null)
                throw new ArgumentNullException(nameof(providers));

            if (defaultSerializer == null)
                throw new ArgumentNullException(nameof(defaultSerializer));

            _providers = providers.ToArray();
            _defaultSerializer = defaultSerializer;
        }

        public T Deserialize<T>(BrokeredMessage entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ContentType == null)
                throw new ArgumentException($"No content type has been set on the brokered message. Unable to source the correct deserializer. Message id: {entity.MessageId}");

            var deserializer = _providers.FirstOrDefault(provider => provider.TypeName.Equals(entity.ContentType, StringComparison.OrdinalIgnoreCase));
            if (deserializer == null)
                throw new Exception($"No deserializer found that is capable of deserializing the type '{entity.ContentType}'. Message id: '{entity.MessageId}'");

            using(var messageBody = entity.GetBody<Stream>())
                return deserializer.Deserialize<T>(messageBody);
        }

        public BrokeredMessage Serialize<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var messageStream = _defaultSerializer.Serialize(entity);

            return new BrokeredMessage(messageStream)
            {
                ContentType = _defaultSerializer.TypeName
            };
        }
    }
}
