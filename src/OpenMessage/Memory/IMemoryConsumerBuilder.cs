using OpenMessage.Configuration;

namespace OpenMessage.Memory
{
    /// <summary>
    /// InMemory consumer builder
    /// </summary>
    /// <typeparam name="T">The type of the message to configure</typeparam>
    public interface IMemoryConsumerBuilder<T> : IBuilder
    {
    }
}
