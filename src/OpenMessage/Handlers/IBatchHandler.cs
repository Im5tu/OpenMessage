using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenMessage.Pipelines;

namespace OpenMessage.Handlers
{
    /// <summary>
    ///     Defines a handler for a batch of messages. See <see cref="PipelineOptions{T}"/> for batch configuration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBatchHandler<T>
    {
        /// <summary>
        ///     Handles a batch of messages
        /// </summary>
        /// <param name="messages">The batch of messages to handle</param>
        /// <param name="cancellationToken">The cancellation token used</param>
        /// <returns>A task that completes when the handle method has completed</returns>
        Task HandleAsync(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken);
    }
}