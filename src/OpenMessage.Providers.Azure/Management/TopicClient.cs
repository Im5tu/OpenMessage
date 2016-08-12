using Microsoft.Extensions.Logging;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.TopicClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class TopicClient<T> : ClientBase<T>, ITopicClient<T>
    {
        private AwaitableLazy<AzureClient> _client;

        public TopicClient(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider,
            ILogger<ClientBase<T>> logger)
            : base(serializationProvider, logger)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            _client = new AwaitableLazy<AzureClient>(async () =>
            {
                await namespaceManager.ProvisionQueueAsync();
                return namespaceManager.CreateTopicClient();
            });
        }

        public async Task SendAsync(T entity, TimeSpan scheduleIn)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (scheduleIn < TimeSpan.Zero)
                throw new ArgumentException("You cannot schedule a message to arrive in the past; time travel isn't a thing yet.");

            var message = Serialize(entity);
            if (scheduleIn > TimeSpan.MinValue)
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow + scheduleIn;

            await (await _client).SendAsync(message);
        }

        public override void Dispose(bool disposing)
        {
            if (_client.IsValueCreated)
                _client.Value?.Close();
        }
    }
}
