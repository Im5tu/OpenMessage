using System.Threading.Tasks;

namespace OpenMessage
{
    /// <summary>
    ///     Extensions for the dispatcher
    /// </summary>
    public static class DispatcherExtensions
    {
        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="entity">The entity to dispatch</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity) => dispatcher.DispatchAsync(entity, default);

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="message">The message to dispatch</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, Message<T> message) => dispatcher.DispatchAsync(message, default);
    }
}