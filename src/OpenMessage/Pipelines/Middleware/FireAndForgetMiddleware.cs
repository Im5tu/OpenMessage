using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Fire and forget
    /// WARNING: exceptions?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FireAndForgetMiddleware<T> : IMiddleware<T>
    {
        private readonly ILogger<FireAndForgetMiddleware<T>> _logger;
        private readonly IOptionsMonitor<PipelineOptions<T>> _options;

        public FireAndForgetMiddleware(ILogger<FireAndForgetMiddleware<T>> logger, IOptionsMonitor<PipelineOptions<T>> options)
        {
            _logger = logger;
            _options = options;
        }

        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            if (_options.CurrentValue.PipelineType == PipelineType.Serial)
            {
                await next(message, cancellationToken, messageContext);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await next(message, cancellationToken, messageContext);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }, cancellationToken);
        }
    }
}