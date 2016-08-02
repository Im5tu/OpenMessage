using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ServiceBus.Messaging;
using OpenMessage.Providers.Azure.Configuration;
using OpenMessage.Providers.Azure.Conventions;
using System;
using System.Threading.Tasks;
using ServiceBus = Microsoft.ServiceBus.NamespaceManager;

namespace OpenMessage.Providers.Azure.Management
{
    public class NamespaceManager<T> : INamespaceManager<T>
    {
        private readonly ILogger<NamespaceManager<T>> _logger;
        private readonly OpenMessageAzureProviderOptions<T> _options;
        private readonly IQueueNamingConvention _queueNamingConvention;
        private readonly ISubscriptionNamingConvention _subscriptionNamingConvention;
        private readonly ITopicNamingConvention _topicNamingConvention;

        public NamespaceManager(IOptions<OpenMessageAzureProviderOptions<T>> options,
            IQueueNamingConvention queueNamingConvention,
            ITopicNamingConvention topicNamingConvention,
            ISubscriptionNamingConvention subscriptionNamingConvention,
            ILogger<NamespaceManager<T>> logger)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (queueNamingConvention == null)
                throw new ArgumentNullException(nameof(queueNamingConvention));

            if (topicNamingConvention == null)
                throw new ArgumentNullException(nameof(topicNamingConvention));

            if (subscriptionNamingConvention == null)
                throw new ArgumentNullException(nameof(subscriptionNamingConvention));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _options = options.Value;
            _logger = logger;
            _queueNamingConvention = queueNamingConvention;
            _topicNamingConvention = topicNamingConvention;
            _subscriptionNamingConvention = subscriptionNamingConvention;
        }

        public QueueClient CreateQueueClient() => QueueClient.CreateFromConnectionString(_options.ConnectionString, _queueNamingConvention.GenerateName<T>(), ReceiveMode.PeekLock);
        public TopicClient CreateTopicClient() => TopicClient.CreateFromConnectionString(_options.ConnectionString, _topicNamingConvention.GenerateName<T>());
        public SubscriptionClient CreateSubscriptionClient() => SubscriptionClient.CreateFromConnectionString(_options.ConnectionString, _topicNamingConvention.GenerateName<T>(), _subscriptionNamingConvention.GenerateName<T>(), ReceiveMode.PeekLock);

        public async Task ProvisionQueueAsync()
        {
            var queueName = _queueNamingConvention.GenerateName<T>();

            var manager = CreateServiceBusManager();

            if (!(await manager.QueueExistsAsync(queueName)))
            {
                _logger.LogDebug($"Provisioning queue: '{queueName}'");

                // TODO :: Implement retry policies
                try
                {
                    await manager.CreateQueueAsync(new QueueDescription(queueName)
                    {
                        AutoDeleteOnIdle = _options.AutoDeleteOnIdle,
                        DefaultMessageTimeToLive = _options.MessageTimeToLive,
                        EnableBatchedOperations = _options.EnableServerSideBatchedOperations,
                        EnableExpress = _options.EnableExpress,
                        EnablePartitioning = _options.EnablePartitioning,
                        LockDuration = _options.MessageLockDuration,
                        MaxDeliveryCount = _options.MaximumDeliveryCount
                    });

                    _logger.LogInformation($"Provisioned queue: '{queueName}'");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occured whilst provisioning queue: '{queueName}'", ex);
                }
            }
            else
                _logger.LogDebug($"Queue '{queueName}' already exists, no need to provision.");
        }
        public async Task ProvisionSubscriptionAsync()
        {
            var topicName = _topicNamingConvention.GenerateName<T>();
            var subscriptionName = _subscriptionNamingConvention.GenerateName<T>();

            var manager = CreateServiceBusManager();

            await ProvisionTopicAsync(manager, topicName);

            if (!(await manager.SubscriptionExistsAsync(topicName, subscriptionName)))
            {
                _logger.LogDebug($"Provisioning subscription: '{topicName}/{subscriptionName}'");

                // TODO :: Implement retry policies
                try
                {
                    await manager.CreateSubscriptionAsync(new SubscriptionDescription(topicName, subscriptionName)
                    {
                        AutoDeleteOnIdle = _options.AutoDeleteOnIdle,
                        DefaultMessageTimeToLive = _options.MessageTimeToLive,
                        EnableBatchedOperations = _options.EnableServerSideBatchedOperations,
                        LockDuration = _options.MessageLockDuration,
                        MaxDeliveryCount = _options.MaximumDeliveryCount
                    });

                    _logger.LogInformation($"Provisioned subscription: '{topicName}/{subscriptionName}'");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occured whilst provisioning subscription: '{topicName}/{subscriptionName}'", ex);
                }
            }
            else
                _logger.LogDebug($"Subscription '{topicName}/{subscriptionName}' already exists, no need to provision.");
        }
        public Task ProvisionTopicAsync() => ProvisionTopicAsync(CreateServiceBusManager(), _topicNamingConvention.GenerateName<T>());
        
        private async Task ProvisionTopicAsync(ServiceBus manager, string topicName)
        {
            if (!(await manager.TopicExistsAsync(topicName)))
            {
                _logger.LogDebug($"Provisioning topic: '{topicName}'");

                // TODO :: Implement retry policies
                try
                {
                    await manager.CreateTopicAsync(new TopicDescription(topicName)
                    {
                        AutoDeleteOnIdle = _options.AutoDeleteOnIdle,
                        DefaultMessageTimeToLive = _options.MessageTimeToLive,
                        EnableBatchedOperations = _options.EnableServerSideBatchedOperations,
                        EnableExpress = _options.EnableExpress,
                        EnablePartitioning = _options.EnablePartitioning
                    });

                    _logger.LogInformation($"Provisioned topic: '{topicName}'");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occured whilst provisioning topic: '{topicName}'", ex);
                }
            }
            else
                _logger.LogDebug($"Topic '{topicName}' already exists, no need to provision.");
        }
        private ServiceBus CreateServiceBusManager()
        {
            var manager = ServiceBus.CreateFromConnectionString(_options.ConnectionString);

            manager.Settings.OperationTimeout = _options.RemoteOperationTimeout;

            return manager;
        }
    }
}
