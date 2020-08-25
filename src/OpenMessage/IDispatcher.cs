using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage
{
    /// <summary>
    ///     Sends a message to a messaging component
    /// </summary>
    /// <typeparam name="T">The type of message to send</typeparam>
    public interface IDispatcher<T>
    {
        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="entity">The entity to dispatch</param>
        /// <param name="cancellationToken">The cancellation token to use where applicable</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        Task DispatchAsync(T entity, CancellationToken cancellationToken);

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="message">The message to dispatch</param>
        /// <param name="cancellationToken">The cancellation token to use where applicable</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        Task DispatchAsync(Message<T> message, CancellationToken cancellationToken);
    }
}