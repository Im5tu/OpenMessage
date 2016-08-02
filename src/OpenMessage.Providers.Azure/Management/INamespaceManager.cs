using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Management
{
    internal interface INamespaceManager<T>
    {
        QueueClient CreateQueueClient();
        TopicClient CreateTopicClient();
        SubscriptionClient CreateSubscriptionClient();

        Task ProvisionQueueAsync();
        Task ProvisionTopicAsync();
        Task ProvisionSubscriptionAsync();
    }
}
