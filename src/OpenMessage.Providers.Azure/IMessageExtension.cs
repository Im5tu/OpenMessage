using Microsoft.ServiceBus.Messaging;

namespace OpenMessage.Providers.Azure
{
    public interface IMessageExtension<in T>
    {
        void Extend(BrokeredMessage message);
    }
}
