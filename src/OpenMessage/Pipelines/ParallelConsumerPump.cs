using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenMessage.Pipelines
{
    internal sealed class ParallelConsumerPump<T> : ConsumerPumpBase<T>
    {
        public ParallelConsumerPump(ChannelReader<Message<T>> channelReader,
            IPipeline<T> pipeline,
            IOptionsMonitor<PipelineOptions<T>> optionsMonitor,
            ILogger<ParallelConsumerPump<T>> logger)
            : base(channelReader, pipeline, optionsMonitor, logger)
        {
        }

        protected override Task OnMessageConsumed(Batch<T> message, Trace.ActivityTracer tracer, CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                using (tracer)
                {
                    await Pipeline.HandleAsync(message, cancellationToken);

                    if (Options.AutoAcknowledge == true && message is ISupportAcknowledgement aam)
                        await aam.AcknowledgeAsync();
                }
            }, cancellationToken);
            return Task.CompletedTask;
        }
    }
}