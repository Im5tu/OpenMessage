using Microsoft.Extensions.Logging;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureClient = Microsoft.ServiceBus.Messaging.TopicClient;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class TopicClient<T> : ClientBase<T>, ITopicClient<T>
    {
        private readonly AwaitableLazy<AzureClient> _client;
        private readonly IMessageExtension<T>[] _extensions;

        public TopicClient(INamespaceManager<T> namespaceManager,
            ISerializationProvider serializationProvider,
            ILogger<ClientBase<T>> logger,
            IEnumerable<IMessageExtension<T>> extensions)
            : base(serializationProvider, logger)
        {
            if (namespaceManager == null)
                throw new ArgumentNullException(nameof(namespaceManager));

            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));

            _extensions = extensions.ToArray();
            _client = new AwaitableLazy<AzureClient>(async () =>
            {
                await namespaceManager.ProvisionTopicAsync();
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

            Logger.LogInformation($"Sending message of type: {TypeName}");
            try
            {
                foreach (var extension in _extensions)
                    extension.Extend(message);

                await (await _client).SendAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error sending message of type: {TypeName}; Error: {ex.Message}", ex);
                throw;
            }
        }

        public override void Dispose(bool disposing)
        {
            if (_client.IsValueCreated)
                _client.Value?.Close();
        }
    }
}
