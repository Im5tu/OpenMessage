using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Pipelines.Middleware;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    /// Pushes a message into the provided pipeline and waits for it to complete
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerialPipelineInitiator<T> : IPipelineInitiator<T>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceScopeFactory"></param>
        public SerialPipelineInitiator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracer"></param>
        /// <param name="pipeline"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Initiate(Trace.ActivityTracer tracer, PipelineDelegate.SingleMiddleware<T> pipeline, Message<T> message, CancellationToken cancellationToken)
        {
            //TODO: should tracer belong in a middleware now?
            using (tracer)
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await pipeline(message, cancellationToken, new MessageContext(scope.ServiceProvider));
            }
        }
    }
}