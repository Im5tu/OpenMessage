using OpenMessage.Providers.Azure.Serialization;
using System;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class TopicFactory<T> : ITopicFactory<T>
    {
        private readonly INamespaceManager<T> _namespaceManager;
        private readonly ISerializationProvider _serializationProvider;

        public TopicFactory(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            if (serializationProvider == null)
                throw new ArgumentNullException(nameof(serializationProvider));

            _namespaceManager = namespaceManager;
            _serializationProvider = serializationProvider;
        }

        public ITopicClient<T> Create() => new TopicClient<T>(_namespaceManager, _serializationProvider);
    }
}
