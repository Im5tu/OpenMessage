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
        /// <param name="batch">The batch of messages to handle</param>
        /// <param name="cancellationToken">The cancellation token to use for asynchronous operations</param>
        /// <returns>A task that completes when the message has been handled</returns>
        Task HandleAsync(Batch<T> batch, CancellationToken cancellationToken);
    }
}