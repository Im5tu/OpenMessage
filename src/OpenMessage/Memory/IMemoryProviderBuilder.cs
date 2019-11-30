using OpenMessage.Builders;
using System;
using System.Threading.Channels;

namespace OpenMessage.Memory
{
    /// <summary>
    ///     The builder for an memory consumer and dispatcher
    /// </summary>
    /// <typeparam name="T">The type to be consumed/dispatched</typeparam>
    public interface IMemoryProviderBuilder<T> : IBuilder
    {
        /// <summary>
        ///     Configures the underlying <see cref="Channel{T}" /> of the memory provider
        /// </summary>
        /// <param name="channelCreator"></param>
        /// <returns></returns>
        IMemoryProviderBuilder<T> ConfigureChannel(Func<IServiceProvider, Channel<Message<T>>> channelCreator);
    }
}