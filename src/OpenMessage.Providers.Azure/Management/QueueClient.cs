using Microsoft.Extensions.Logging;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.QueueClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class QueueClient<T> : ClientBase<T>, IQueueClient<T>
    {
        private readonly INamespaceManager<T> _namespaceManager;
        private Task _clientCreationTask;
        private AzureClient _client;

        public QueueClient(INamespaceManager<T> namespaceManager,
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

        public async Task SendAsync(T entity, TimeSpan scheduleIn)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (scheduleIn < TimeSpan.Zero)
                throw new ArgumentException("You cannot schedule a message to arrive in the past; time travel isn't a thing yet.");
            
            await EnsureClientIsReady();

            var message = Serialize(entity);
            if (scheduleIn > TimeSpan.Zero)
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(scheduleIn);

            await _client.SendAsync(message);
        }

        private async Task EnsureClientIsReady()
        {
            if (_clientCreationTask.IsCompleted)
                return;

            if (_clientCreationTask.IsFaulted || _clientCreationTask.IsCanceled)
                _clientCreationTask = CreateClient();

            await _clientCreationTask;
        }

        private async Task CreateClient()
        {
            if (_client == null)
            {
                await _namespaceManager.ProvisionQueueAsync();
                _client = _namespaceManager.CreateQueueClient();
                _client.OnMessage(OnMessage);
            }
        }

        public override void Dispose(bool disposing) => _client?.Close();
    }
}