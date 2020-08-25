using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Base class for handling null messages and expired cancellation tokens
    /// </summary>
    public abstract class Middleware<T> : IMiddleware<T>
    {
        /// <inheritdoc />
        public Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            if (message is null)
                Throw.ArgumentNullException(nameof(message));

            if (messageContext is null)
                Throw.ArgumentNullException(nameof(messageContext));

            cancellationToken.ThrowIfCancellationRequested();

            return OnInvoke(message, cancellationToken, messageContext, next);
        }

        /// <summary>
        ///     Invokes the middleware
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <param name="cancellationToken">The current cancellation token</param>
        /// <param name="messageContext">The context of the message</param>
        /// <param name="next">The next middleware to run</param>
        protected abstract Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next);
    }
}