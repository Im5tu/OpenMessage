using OpenMessage.Handlers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    ///     Calls all <see cref="IHandler{T}" /> and <see cref="IBatchHandler{T}" />
    /// </summary>
    public class BatchHandlerPipelineEndpoint<T> : IBatchPipelineEndpoint<T>
    {
        private readonly IEnumerable<IBatchHandler<T>> _batchHandlers;
        private readonly IEnumerable<IHandler<T>> _handlers;

        /// <summary>
        ///     ctor
        /// </summary>
        public BatchHandlerPipelineEndpoint(IEnumerable<IHandler<T>> handlers, IEnumerable<IBatchHandler<T>> batchHandlers)
        {
            _handlers = handlers;
            _batchHandlers = batchHandlers;
        }

        /// <summary>
        ///     Calls all handlers with the batch of messages
        /// </summary>
        public Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext) => Task.WhenAll(Invoke(messages, cancellationToken));

        private IEnumerable<Task> Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken)
        {
            foreach (var handler in _batchHandlers)
                yield return handler.HandleAsync(messages, cancellationToken);

            foreach (var message in messages)
            foreach (var handler in _handlers)
                yield return handler.HandleAsync(message, cancellationToken);
        }
    }
}