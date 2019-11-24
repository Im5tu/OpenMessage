using OpenMessage.Configuration;

namespace OpenMessage.Memory
{
    /// <summary>
    /// InMemory dispatcher builder
    /// </summary>
    /// <typeparam name="T">The type of the message to configure</typeparam>
    public interface IMemoryDispatcherBuilder<T> : IBuilder
    {
    }
}