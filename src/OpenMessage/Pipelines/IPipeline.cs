using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines
{
    /// <summary>
    ///     Defines a chain that each message will be processed through
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipeline<T>
    {
        /// <summary>
        ///     Handles the specified message
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <param name="cancellationToken">The cancellation token to use for asynchronous operations</param>
        /// <returns>A task that completes when the message has been handled</returns>
        Task HandleAsync(Message<T> message, CancellationToken cancellationToken);
    }
}