using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Handlers
{
    /// <summary>
    ///     Defines a handler for a batch of messages
    /// </summary>
    /// <typeparam name="T">The type contained in a message</typeparam>
    public interface IBatchHandler<T>
    {
        /// <summary>
        ///     Handles the batch of messages
        /// </summary>
        /// <param name="messages">Messages to be handled</param>
        /// <param name="cancellationToken">The cancellation token used</param>
        /// <returns>A task that completes when the handle method has completed</returns>
        Task HandleAsync(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken);
    }
}