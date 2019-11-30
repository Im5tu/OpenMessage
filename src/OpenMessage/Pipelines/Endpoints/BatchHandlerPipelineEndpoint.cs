using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenMessage.Handlers;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    /// Calls all <see cref="IHandler{T}"/> and <see cref="IBatchHandler{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchHandlerPipelineEndpoint<T> : IBatchPipelineEndpoint<T>
    {
        private readonly IEnumerable<IHandler<T>> _handlers;
        private readonly IEnumerable<IBatchHandler<T>> _batchHandlers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="batchHandlers"></param>
        public BatchHandlerPipelineEndpoint(IEnumerable<IHandler<T>> handlers, IEnumerable<IBatchHandler<T>> batchHandlers)
        {
            _handlers = handlers;
            _batchHandlers = batchHandlers;
        }

        /// <summary>
        /// Calls all handlers with the batch of messages
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        public Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext)
        {
            return Task.WhenAll(Invoke(messages, cancellationToken));
        }

        /// <summary>
        /// TODO: Should we check the <see cref="CancellationToken"/> within the for loops?
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerable<Task> Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken)
        {
            foreach (var handler in _batchHandlers)
            {
                yield return handler.HandleAsync(messages, cancellationToken);
            }

            foreach (var message in messages)
            {
                foreach (var handler in _handlers)
                {
                    yield return handler.HandleAsync(message, cancellationToken);
                }
            }
        }
    }
}