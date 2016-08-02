using Nito.AsyncEx;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.SubscriptionClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class SubscriptionClient<T> : ClientBase<T>, ISubscriptionClient<T>
    {
        private readonly INamespaceManager<T> _namespaceManager;
        private readonly AsyncLock _mutex = new AsyncLock();
        private AzureClient _client;

        public SubscriptionClient(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider)
            : base(serializationProvider)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            _namespaceManager = namespaceManager;
        }

        public void RegisterCallback(Action<T> callback)
        {
            if (_client == null)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CreateClient().ContinueWith(tsk => _client.OnMessage(OnMessage), TaskContinuationOptions.OnlyOnRanToCompletion);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            AddCallback(callback);
        }

        private async Task CreateClient()
        {
            using (await _mutex.LockAsync())
            {
                if (_client == null)
                {
                    await _namespaceManager.ProvisionSubscriptionAsync();
                    _client = _namespaceManager.CreateSubscriptionClient();
                }
            }
        }

        public override void Dispose(bool disposing) => _client?.Close();
    }
}
