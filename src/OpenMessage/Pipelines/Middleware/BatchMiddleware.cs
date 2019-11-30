using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Base class for handling null messages and expired cancellation tokens
    /// </summary>
    public abstract class BatchMiddleware<T> : IBatchMiddleware<T>
    {
        /// <inheritdoc />
        public Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.BatchMiddleware<T> next)
        {
            if (messages is null)
                Throw.ArgumentNullException(nameof(messages));

            if (messageContext is null)
                Throw.ArgumentNullException(nameof(messageContext));

            cancellationToken.ThrowIfCancellationRequested();

            return OnInvoke(messages, cancellationToken, messageContext, next);
        }

        /// <summary>
        ///     Invokes the middleware
        /// </summary>
        /// <param name="messages">The messages to handle</param>
        /// <param name="cancellationToken">The current cancellation token</param>
        /// <param name="messageContext">The context of the message</param>
        /// <param name="next">The next middleware to run</param>
        protected abstract Task OnInvoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.BatchMiddleware<T> next);
    }
}