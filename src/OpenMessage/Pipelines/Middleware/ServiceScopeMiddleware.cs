using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Creates a new service scope
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceScopeMiddleware<T> : IMiddleware<T>
    {
        /// <inheritdoc />
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            using (var scope = messageContext.ServiceProvider.CreateScope())
            {
                await next(message, cancellationToken, new MessageContext(scope.ServiceProvider));
            }    
        }
    }
}
