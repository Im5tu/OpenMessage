using System;
using System.Threading.Channels;

namespace OpenMessage.Memory
{
    /// <summary>
    /// The builder for an memory consumer and dispatcher
    /// </summary>
    /// <typeparam name="T">The type to be consumed/dispatched</typeparam>
    public interface IMemoryProviderBuilder<T>
    {
        /// <summary>
        /// Configures the <see cref="MemoryOptions{T}"/> of the dispatcher and consumer
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IMemoryProviderBuilder<T> ConfigureOptions(Action<MemoryOptions<T>> options);

        /// <summary>
        /// Configures the underlying <see cref="Channel{T}"/> of the memory provider
        /// </summary>
        /// <param name="channelCreator"></param>
        /// <returns></returns>
        IMemoryProviderBuilder<T> ConfigureChannel(Func<IServiceProvider, Channel<Message<T>>> channelCreator);

        /// <summary>
        /// Builds the provider and adds it to the container
        /// </summary>
        void Build();
    }
}