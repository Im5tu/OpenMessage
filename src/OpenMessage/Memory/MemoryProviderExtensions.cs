using System;
using System.Threading.Channels;
using OpenMessage;
using OpenMessage.Extensions;
using OpenMessage.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for adding an InMemory Dispatcher/Consumer
    /// </summary>
    public static class MemoryProviderExtensions
    {
        /// <summary>
        ///     Adds an InMemory dispatcher
        /// </summary>
        /// <param name="builder">The current host builder</param>
        /// <param name="channelCreator">The function to use to create the underlying channel</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>An instance of <see cref="IMessagingBuilder" /></returns>
        public static IMessagingBuilder ConfigureMemoryDispatcher<T>(this IMessagingBuilder builder, Func<IServiceProvider, Channel<Message<T>>> channelCreator = null)
        {
            builder.Services.TryAddChannel(channelCreator).AddSingleton<IDispatcher<T>, MemoryDispatcher<T>>();
            return builder;
        }

        /// <summary>
        ///     Adds an InMemory consumer
        /// </summary>
        /// <param name="builder">The current host builder</param>
        /// <param name="channelCreator">The function to use to create the underlying channel</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>An instance of <see cref="IMessagingBuilder" /></returns>
        public static IMessagingBuilder ConfigureMemoryConsumer<T>(this IMessagingBuilder builder, Func<IServiceProvider, Channel<Message<T>>> channelCreator = null)
        {
            builder.Services.TryAddConsumerService<T>(channelCreator);
            return builder;
        }

        /// <summary>
        ///     Adds an InMemory consumer and dispatcher
        /// </summary>
        /// <param name="builder">The current host builder</param>
        /// <param name="channelCreator">The function to use to create the underlying channel</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>An instance of <see cref="IMessagingBuilder" /></returns>
        public static IMessagingBuilder ConfigureMemory<T>(this IMessagingBuilder builder, Func<IServiceProvider, Channel<Message<T>>> channelCreator = null)
        {
            return builder.ConfigureMemoryConsumer(channelCreator).ConfigureMemoryDispatcher(channelCreator);
        }
    }
}