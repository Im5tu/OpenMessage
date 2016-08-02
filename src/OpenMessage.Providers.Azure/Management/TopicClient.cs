using Nito.AsyncEx;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.TopicClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class TopicClient<T> : ClientBase<T>, ITopicClient<T>
    {
        private readonly INamespaceManager<T> _namespaceManager;
        private readonly AsyncLock _mutex = new AsyncLock();
        private AzureClient _client;

        public TopicClient(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider)
            : base(serializationProvider)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            _namespaceManager = namespaceManager;
        }

        public async Task SendAsync(T entity, TimeSpan scheduleIn)
        {
            // TODO :: argument check
            if (_client == null)
                await CreateClient();

            var message = Serialize(entity);

            if (scheduleIn > TimeSpan.MinValue)
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow + scheduleIn;

            await _client.SendAsync(message);
        }

        private async Task CreateClient()
        {
            // TODO :: early exit
            using (await _mutex.LockAsync())
            {
                if (_client == null)
                {
                    await _namespaceManager.ProvisionTopicAsync();
                    _client = _namespaceManager.CreateTopicClient();
                }
            }
        }

        public override void Dispose(bool disposing) => _client?.Close();
    }
}
