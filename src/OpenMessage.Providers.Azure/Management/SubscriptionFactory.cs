using Microsoft.Extensions.Logging;
using OpenMessage.Providers.Azure.Serialization;
using System;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class SubscriptionFactory<T> : ISubscriptionFactory<T>
    {
        private readonly ILogger<ClientBase<T>> _logger;
        private readonly INamespaceManager<T> _namespaceManager;
        private readonly ISerializationProvider _serializationProvider;

        public SubscriptionFactory(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider,
            ILogger<ClientBase<T>> logger)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            if (serializationProvider == null)
                throw new ArgumentNullException(nameof(serializationProvider));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _namespaceManager = namespaceManager;
            _serializationProvider = serializationProvider;
            _logger = logger;
        }

        public ISubscriptionClient<T> Create() => new SubscriptionClient<T>(_namespaceManager, _serializationProvider, _logger);
    }
}
