using Microsoft.Extensions.Logging;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.SubscriptionClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class SubscriptionClient<T> : ClientBase<T>, ISubscriptionClient<T>
    {
        private readonly INamespaceManager<T> _namespaceManager;
        private readonly Task _clientCreationTask;
        private AzureClient _client;

        public SubscriptionClient(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider,
            ILogger<ClientBase<T>> logger)
            : base(serializationProvider, logger)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            _namespaceManager = namespaceManager;
            _clientCreationTask = CreateClient();
        }

        public void RegisterCallback(Action<T> callback) => AddCallback(callback);

        private async Task CreateClient()
        {
            if (_client == null)
            {
                await _namespaceManager.ProvisionSubscriptionAsync();
                _client = _namespaceManager.CreateSubscriptionClient();
                _client.OnMessage(OnMessage);
            }
        }

        public override void Dispose(bool disposing) => _client?.Close();
    }
}
