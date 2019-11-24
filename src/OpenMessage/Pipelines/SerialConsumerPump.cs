using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenMessage.Pipelines
{
    internal sealed class SerialConsumerPump<T> : ConsumerPumpBase<T>
    {
        public SerialConsumerPump(ChannelReader<Message<T>> channelReader,
            IPipeline<T> pipeline,
            IOptionsMonitor<PipelineOptions<T>> optionsMonitor,
            ILogger<SerialConsumerPump<T>> logger)
            : base(channelReader, pipeline, optionsMonitor, logger)
        {
        }

        protected override async Task OnMessageConsumed(Batch<T> message, Trace.ActivityTracer tracer, CancellationToken cancellationToken)
        {
            using (tracer)
            {
                await Pipeline.HandleAsync(message, cancellationToken);

                if (Options.AutoAcknowledge == true && message is ISupportAcknowledgement aam)
                    await aam.AcknowledgeAsync();
            }
        }
    }
}