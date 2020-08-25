using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Middleware contract for a batched pipeline
    /// </summary>
    public interface IBatchMiddleware<T>
    {
        /// <summary>
        ///     Invokes the middleware
        /// </summary>
        /// <param name="messages">The messages to handle</param>
        /// <param name="cancellationToken">The current cancellation token</param>
        /// <param name="messageContext">The context of the message</param>
        /// <param name="next">The next middleware to run</param>
        Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.BatchMiddleware<T> next);
    }
}