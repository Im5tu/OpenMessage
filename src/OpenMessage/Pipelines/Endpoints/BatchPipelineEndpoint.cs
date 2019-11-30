using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Pipelines.Builders;

namespace OpenMessage.Pipelines.Endpoints
{
    internal class BatchPipelineEndpoint<T> : BatcherBase<Message<T>>, IPipelineEndpoint<T>
    {
        private readonly ILogger<BatchPipelineEndpoint<T>> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBatchPipelineBuilder<T> _batchPipelineBuilder;

        public BatchPipelineEndpoint(ILogger<BatchPipelineEndpoint<T>> logger, IServiceScopeFactory serviceScopeFactory, IBatchPipelineBuilder<T> batchPipelineBuilder, IOptions<PipelineOptions<T>> options)
            : base(options.Value.BatchSize, options.Value.BatchTimeout)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _batchPipelineBuilder = batchPipelineBuilder;
        }

        /// <summary>
        /// Push all messages into a batch
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext)
        {
            await BatchAsync(message);
        }

        /// <summary>
        /// When the batch is full, then build a batch pipeline and pass it through
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        protected override async Task OnBatchAsync(IReadOnlyCollection<Message<T>> batch)
        {
            var batchPipeline = _batchPipelineBuilder.Build();

            using (var scope = _serviceScopeFactory.CreateScope())
            using (_logger.BeginScope($"{nameof(BatchPipelineEndpoint<T>)} - {batch.Count} Messages"))
            {
                //batches no longer support their cancellation token
                await batchPipeline(batch, new CancellationToken(), new MessageContext(scope.ServiceProvider));
            }
        }
    }
}
