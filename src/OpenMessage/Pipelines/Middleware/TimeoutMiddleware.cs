using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Triggers the cancellation token after a given timeout
    /// </summary>
    public class TimeoutMiddleware<T> : Middleware<T>
    {
        private readonly IOptionsMonitor<PipelineOptions<T>> _optionsMonitor;

        /// <inheritdoc />
        public TimeoutMiddleware(IOptionsMonitor<PipelineOptions<T>> optionsMonitor) => _optionsMonitor = optionsMonitor;

        /// <inheritDoc />
        protected override async Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            using var timedCts = new CancellationTokenSource(_optionsMonitor.CurrentValue.PipelineTimeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timedCts.Token);

            await next(message, cts.Token, messageContext);
        }
    }
}