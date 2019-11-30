using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Endpoints;

namespace OpenMessage.MediatR
{
    /// <summary>
    /// A pipeline endpoint that calls into MediatR
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MediatRPipelineEndpoint<T> : IPipelineEndpoint<T>
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public MediatRPipelineEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <inheritdoc />
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext)
        {
            await _mediator.Publish<MediatRMessage<T>>(message, cancellationToken);
        }
    }
}
