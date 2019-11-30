using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Endpoints;

namespace OpenMessage.MediatR
{
    /// <summary>
    /// A batch pipeline endpoint that calls into MediatR
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MediatRBatchPipelineEndpoint<T> : IBatchPipelineEndpoint<T>
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public MediatRBatchPipelineEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <inheritdoc />
        public async Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext)
        {
            await _mediator.Publish(new MediatRBatch<T>(messages), cancellationToken);

            foreach (var message in messages)
            {
                await _mediator.Publish<MediatRMessage<T>>(message, cancellationToken);
            }
        }
    }
}