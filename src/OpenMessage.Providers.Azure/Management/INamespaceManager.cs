using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Management
{
    internal interface INamespaceManager<T>
    {
        Task ProvisionQueueAsync();
        Task ProvisionTopicAsync();
        Task ProvisionSubscriptionAsync();
    }
}
