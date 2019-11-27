using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenMessage.Handlers;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    /// Calls all <see cref="IHandler{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HandlerPipelineEndpoint<T> : IPipelineEndpoint<T>
    {
        private readonly IEnumerable<IHandler<T>> _handlers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlers"></param>
        public HandlerPipelineEndpoint(IEnumerable<IHandler<T>> handlers)
        {
            _handlers = handlers;
        }

        /// <summary>
        /// Calls all handlers with the given message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        public Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext)
        {
            return Task.WhenAll(_handlers.Select(x => x.HandleAsync(message, cancellationToken)));
        }
    }
}
