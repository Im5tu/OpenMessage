using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Creates a new service scope
    /// </summary>
    public class ServiceScopeMiddleware<T> : Middleware<T>
    {
        /// <inheritDoc />
        protected override async Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            using var scope = messageContext.ServiceProvider.CreateScope();
            await next(message, cancellationToken, new MessageContext(scope.ServiceProvider));
        }
    }
}