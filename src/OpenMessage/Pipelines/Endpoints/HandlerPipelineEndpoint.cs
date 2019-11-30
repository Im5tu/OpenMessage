using OpenMessage.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    ///     Calls all <see cref="IHandler{T}" />
    /// </summary>
    public class HandlerPipelineEndpoint<T> : IPipelineEndpoint<T>
    {
        private readonly IEnumerable<IHandler<T>> _handlers;

        /// <summary>
        ///     ctor
        /// </summary>
        public HandlerPipelineEndpoint(IEnumerable<IHandler<T>> handlers) => _handlers = handlers;

        /// <summary>
        ///     Calls all handlers with the given message
        /// </summary>
        public Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext)
        {
            return Task.WhenAll(_handlers.Select(x => x.HandleAsync(message, cancellationToken)));
        }
    }
}