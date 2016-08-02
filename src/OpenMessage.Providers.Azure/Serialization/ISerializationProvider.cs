using Microsoft.ServiceBus.Messaging;
using System.IO;

namespace OpenMessage.Providers.Azure.Serialization
{
    internal interface ISerializationProvider
    {
        BrokeredMessage Serialize<T>(T entity);
        T Deserialize<T>(BrokeredMessage entity);
    }
}
