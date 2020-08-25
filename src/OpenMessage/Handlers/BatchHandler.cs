using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Handlers
{
    /// <summary>
    ///     Standard handler that checks for empty collections and cancelled cancellation tokens
    /// </summary>
    /// <typeparam name="T">The type to handle</typeparam>
    public abstract class BatchHandler<T> : IBatchHandler<T>
    {
        /// <summary>
        ///     The logger passed in
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="logger">The logger to use</param>
        protected BatchHandler(ILogger logger) => Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public Task HandleAsync(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken)
        {
            if (messages is null || messages.Count == 0)
                return Task.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            return OnHandleAsync(messages, cancellationToken);
        }

        /// <summary>
        ///     Handles the specified messages
        /// </summary>
        /// <param name="messages">The messages to handle</param>
        /// <param name="cancellationToken">The cancellation token used</param>
        /// <returns>A task that completes when the handle method has completed</returns>
        protected abstract Task OnHandleAsync(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken);
    }
}