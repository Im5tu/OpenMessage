using Microsoft.Extensions.Options;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Middleware;
using Polly;
using Polly.Registry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Polly
{
    internal sealed class PollyMiddleware<T> : Middleware<T>
    {
        private readonly IOptionsMonitor<PollyMiddlewareOptions<T>> _optionsMonitor;
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;

        public PollyMiddleware(IOptionsMonitor<PollyMiddlewareOptions<T>> optionsMonitor, IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
        }

        /// <inheritdoc />
        protected override async Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            if (!string.IsNullOrWhiteSpace(_optionsMonitor.CurrentValue.PolicyName) && _policyRegistry.TryGet<IAsyncPolicy>(_optionsMonitor.CurrentValue.PolicyName, out var policy))
            {
                await policy.ExecuteAsync(async ct => await next(message, ct, messageContext), cancellationToken);

                return;
            }

            await next(message, cancellationToken, messageContext);
        }
    }
}