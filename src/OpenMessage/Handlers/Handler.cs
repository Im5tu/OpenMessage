using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Handlers
{
    /// <summary>
    ///     Standard handler that checks for null messages and cancelled cancellation tokens
    /// </summary>
    /// <typeparam name="T">The type to handle</typeparam>
    public abstract class Handler<T> : IHandler<T>
    {
        /// <summary>
        ///     The logger passed in
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="logger">The logger to use</param>
        protected Handler(ILogger logger) => Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public Task HandleAsync(Message<T> message, CancellationToken cancellationToken)
        {
            if (message is null)
                Throw.ArgumentNullException(nameof(message));

            cancellationToken.ThrowIfCancellationRequested();

            return OnHandleAsync(message, cancellationToken);
        }

        /// <summary>
        ///     Handles the specified message
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <param name="cancellationToken">The cancellation token used</param>
        /// <returns>A task that completes when the handle method has completed</returns>
        protected abstract Task OnHandleAsync(Message<T> message, CancellationToken cancellationToken);
    }
}