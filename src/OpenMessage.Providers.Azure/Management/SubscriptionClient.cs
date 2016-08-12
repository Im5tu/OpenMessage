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

        public void RegisterCallback(Action<T> callback)
        {
            if (!_clientCreationTask.IsCompleted)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // TODO :: what to do if client creation fails? 
                _clientCreationTask.ContinueWith(tsk => _client.OnMessage(OnMessage), TaskContinuationOptions.OnlyOnRanToCompletion);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            AddCallback(callback);
        }

        private async Task CreateClient()
        {
            if (_client == null)
            {
                await _namespaceManager.ProvisionSubscriptionAsync();
                _client = _namespaceManager.CreateSubscriptionClient();
            }
        }

        public override void Dispose(bool disposing) => _client?.Close();
    }
}
