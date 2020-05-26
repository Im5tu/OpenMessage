using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Pipelines.Builders;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    internal sealed class BatchPipelineEndpoint<T> : BatcherBase<Message<T>>, IPipelineEndpoint<T>
    {
        private readonly IBatchPipelineBuilder<T> _batchPipelineBuilder;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BatchPipelineEndpoint(IServiceScopeFactory serviceScopeFactory, IBatchPipelineBuilder<T> batchPipelineBuilder, IOptions<PipelineOptions<T>> options)
            : base(options.Value.BatchSize, options.Value.BatchTimeout)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _batchPipelineBuilder = batchPipelineBuilder;
        }

        /// <inheritdoc />
        /// >
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext)
        {
            await BatchAsync(message);
        }

        /// <summary>
        ///     When the batch is full, then build a batch pipeline and pass it through
        /// </summary>
        protected override async Task OnBatchAsync(IReadOnlyCollection<Message<T>> batch)
        {
            var batchPipeline = _batchPipelineBuilder.Build();

            using var scope = _serviceScopeFactory.CreateScope();

            //batches no longer support their cancellation token
            await batchPipeline(batch, new CancellationToken(), new MessageContext(scope.ServiceProvider));
        }
    }
}