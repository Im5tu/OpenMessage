using OpenMessage.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for adding an InMemory Dispatcher/Consumer
    /// </summary>
    public static class MemoryProviderExtensions
    {
        /// <summary>
        ///     Adds an InMemory consumer and dispatcher
        /// </summary>
        /// <param name="builder">The current host builder</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>An instance of <see cref="IMemoryProviderBuilder{T}" /></returns>
        public static IMemoryProviderBuilder<T> ConfigureMemory<T>(this IMessagingBuilder builder)
        {
            return new MemoryProviderBuilder<T>(builder);
        }
    }
}