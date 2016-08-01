using System;

namespace OpenMessage.Providers.Azure.Configuration
{
    public class OpenMessageAzureProviderOptions
    {
        /// <summary>
        ///     The connection string to use when connection to the Azure Service Bus Namespace.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        ///     The timeout period for operations that occur in Azure.
        /// </summary>
        public TimeSpan RemoteOperationTimeout { get; set; } = TimeSpan.FromSeconds(5);
        /// <summary>
        ///     Sets the idle interval after which the queue/topic/subscription is automatically deleted. The minimum duration is 5 minutes.
        /// </summary>
        public TimeSpan AutoDeleteOnIdle { get; set; } = TimeSpan.MaxValue;
        /// <summary>
        ///     Determines whether or not to enable the queue to be partitioned across multiple message brokers. An express queue holds a message in memory temporarily before writing it to persistent storage.
        /// </summary>
        public bool EnableExpress { get; set; } = true;
        /// <summary>
        ///     Determines whether or not the queue should be partitioned across multiple message brokers when enabled.
        /// </summary>
        public bool EnablePartitioning { get; set; } = true;
        /// <summary>
        ///     Determines whether server-side batched operations are enabled.
        /// </summary>
        public bool EnableServerSideBatchedOperations { get; set; } = true;
        /// <summary>
        ///     Sets the maximum delivery count. A message is automatically deadlettered after this number of deliveries.
        /// </summary>
        public int MaximumDeliveryCount { get; set; } = 10;
        /// <summary>
        ///     Gets or sets the duration of a peek lock; that is, the amount of time that the message is locked for other receivers. The maximum value for is 5 minutes; the default value is 1 minute.
        /// </summary>
        public TimeSpan MessageLockDuration { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        ///     Gets or sets the default message time to live value. This is the duration after which the message expires, starting from when the message is sent to Service Bus.
        /// </summary>
        public TimeSpan MessageTimeToLive { get; set; } = TimeSpan.MaxValue;
    }
}
