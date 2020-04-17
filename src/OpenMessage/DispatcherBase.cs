using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage
{
    /// <summary>
    ///     Base implementation for all dispatchers
    /// </summary>
    /// <typeparam name="T">The type to dispatch</typeparam>
    public abstract class DispatcherBase<T> : IDispatcher<T>
    {
        private readonly ILogger _logger;
        private readonly Action<ILogger, string, Exception?> _dispatchMessage;

        protected DispatcherBase(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dispatchMessage = LoggerMessage.Define<string>(LogLevel.Debug, 0, "Dispatching message with id: '{0}'");
        }

        /// <inheritdoc />
        public Task DispatchAsync(T entity, CancellationToken cancellationToken) => DispatchAsync(new Message<T>
        {
            Value = entity
        }, cancellationToken);

        /// <inheritdoc />
        public abstract Task DispatchAsync(Message<T> message, CancellationToken cancellationToken);

        /// <summary>
        ///    Logs the message id that's being dispatched
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LogDispatch(Message<T> message)
        {
            if (_logger.IsEnabled(LogLevel.Debug) && message is ISupportIdentification msgId)
                _dispatchMessage(_logger, msgId.Id ?? string.Empty, null);
        }
    }
}