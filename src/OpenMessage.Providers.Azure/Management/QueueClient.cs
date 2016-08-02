using Nito.AsyncEx;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.QueueClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class QueueClient<T> : ClientBase<T>, IQueueClient<T>
    {
        private readonly INamespaceManager<T> _namespaceManager;
        private readonly AsyncLock _mutex = new AsyncLock();
        private AzureClient _client;

        public QueueClient(INamespaceManager<T> namespaceManager,
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

        public async Task SendAsync(T entity, TimeSpan scheduleIn)
        {
            if (_client == null)
                await CreateClient();

            var message = Serialize(entity);

            if (scheduleIn > TimeSpan.MinValue)
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow + scheduleIn;

            await _client.SendAsync(message);
        }

        private async Task CreateClient()
        {
            using (await _mutex.LockAsync())
            {
                if (_client == null)
                {
                    await _namespaceManager.ProvisionQueueAsync();
                    _client = _namespaceManager.CreateQueueClient();
                }
            }
        }

        public override void Dispose(bool disposing) => _client?.Close();
    }
}