using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Middleware contract for a pipeline
    /// </summary>
    public interface IMiddleware<T>
    {
        /// <summary>
        ///     Invokes the middleware
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <param name="cancellationToken">The current cancellation token</param>
        /// <param name="messageContext">The context of the message</param>
        /// <param name="next">The next middleware to run</param>
        Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next);
    }
}