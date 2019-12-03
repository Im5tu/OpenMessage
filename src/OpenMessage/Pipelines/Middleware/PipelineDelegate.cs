using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Delegates for representing pipeline middleware
    /// </summary>
    public static class PipelineDelegate
    {
        /// <summary>
        ///     Represents an invokable pipeline for batches of messages
        /// </summary>
        public delegate Task BatchMiddleware<T>(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext context);

        /// <summary>
        ///     Represents an invokable pipeline for messages
        /// </summary>
        public delegate Task SingleMiddleware<T>(Message<T> message, CancellationToken cancellationToken, MessageContext context);
    }
}