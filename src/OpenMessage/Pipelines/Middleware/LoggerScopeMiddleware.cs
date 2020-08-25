using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Starts a new logger scope using the <see cref="ISupportIdentification" /> if it exists
    /// </summary>
    public class LoggerScopeMiddleware<T> : Middleware<T>
    {
        private static readonly string ScopePrefix = "MessageId";
        private readonly ILogger<LoggerScopeMiddleware<T>> _logger;

        /// <inheritdoc />
        public LoggerScopeMiddleware(ILogger<LoggerScopeMiddleware<T>> logger) => _logger = logger;

        /// <inheritDoc />
        protected override async Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            IDisposable? scope = null;

            if (message is ISupportIdentification identifier && !string.IsNullOrEmpty(identifier?.Id))
                scope = _logger.BeginScope(new KeyValuePair<string, string>(ScopePrefix, identifier.Id));

            using (scope)
            {
                await next(message, cancellationToken, messageContext);
            }
        }
    }
}