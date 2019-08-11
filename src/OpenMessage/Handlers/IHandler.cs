using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Handlers
{
    /// <summary>
    ///     Defines a handler for a message
    /// </summary>
    /// <typeparam name="T">The type contained in a message</typeparam>
    public interface IHandler<T>
    {
        /// <summary>
        ///     Handles the specified message
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <param name="cancellationToken">The cancellation token used</param>
        /// <returns>A task that completes when the handle method has completed</returns>
        Task HandleAsync(Message<T> message, CancellationToken cancellationToken);
    }
}