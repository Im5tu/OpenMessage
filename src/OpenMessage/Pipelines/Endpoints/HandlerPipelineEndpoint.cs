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
        private readonly IHandler<T>[] _handlers;

        /// <summary>
        ///     ctor
        /// </summary>
        public HandlerPipelineEndpoint(IEnumerable<IHandler<T>> handlers) => _handlers = handlers.ToArray();

        /// <summary>
        ///     Calls all handlers with the given message
        /// </summary>
        public Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext)
        {
            if (_handlers.Length == 0)
                Throw.Exception("No handlers found for type: " + TypeCache<T>.FriendlyName);

            if (_handlers.Length == 1)
                return _handlers[0].HandleAsync(message, cancellationToken);

            return Task.WhenAll(_handlers.Select(x => x.HandleAsync(message, cancellationToken)));
        }
    }
}