using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Triggers the cancellation token after a given timeout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeoutMiddleware<T> : IMiddleware<T>
    {
        private readonly IOptionsMonitor<PipelineOptions<T>> _optionsMonitor;

        /// <inheritdoc />
        public TimeoutMiddleware(IOptionsMonitor<PipelineOptions<T>> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        /// <inheritdoc />
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            using var timedCts = new CancellationTokenSource(_optionsMonitor.CurrentValue.PipelineTimeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timedCts.Token);

            await next(message, cts.Token, messageContext);
        }
    }
}