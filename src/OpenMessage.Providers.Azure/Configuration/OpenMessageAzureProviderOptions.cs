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
    }
}
