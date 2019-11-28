using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Starts a new logger scope using the <see cref="ISupportIdentification"/> if it exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoggerScopeMiddleware<T> : IMiddleware<T>
    {
        private readonly ILogger<LoggerScopeMiddleware<T>> _logger;

        /// <inheritdoc />
        public LoggerScopeMiddleware(ILogger<LoggerScopeMiddleware<T>> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            var scopeName = "OpenMessagePipeline";

            if (message is ISupportIdentification identifier)
            {
                scopeName = $"{scopeName}:{identifier}";
            }

            using (_logger.BeginScope(scopeName))
            {
                await next(message, cancellationToken, messageContext);
            }
        }
    }
}