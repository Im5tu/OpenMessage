using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenMessage.Pipelines
{
    /// <summary>
    ///     The base type for providing a consumer of the internal messaging channel
    /// </summary>
    /// <typeparam name="T">The type that is contained in the batch</typeparam>
    internal abstract class ConsumerPumpBase<T> : BackgroundService
    {
        private static readonly string ConsumeActivityName = "OpenMessage.Consumer.Process";

        /// <summary>
        ///     The reader of the messaging channel
        /// </summary>
        protected ChannelReader<Message<T>> ChannelReader { get; }

        /// <summary>
        ///     The pipeline that gets called for each batch
        /// </summary>
        protected IPipeline<T> Pipeline { get; }

        /// <summary>
        ///     The current options for the pipeline
        /// </summary>
        protected IOptionsMonitor<PipelineOptions<T>> OptionsMonitor { get; }

        /// <summary>
        ///     The current options for the pipeline
        /// </summary>
        protected PipelineOptions<T> Options => OptionsMonitor.CurrentValue;

        /// <summary>
        ///     The logger to use
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        protected ConsumerPumpBase(ChannelReader<Message<T>> channelReader,
            IPipeline<T> pipeline,
            IOptionsMonitor<PipelineOptions<T>> optionsMonitor,
            ILogger logger)
        {
            ChannelReader = channelReader ?? throw new ArgumentNullException(nameof(channelReader));
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritDoc />
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting consumer pump: " + GetType().GetFriendlyName());

            return base.StartAsync(cancellationToken);
        }

        /// <inheritDoc />
        protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && !ChannelReader.Completion.IsCompleted)
                {
                    Batch<T> batch = null;
                    try
                    {
                        var messages = await ReadBatchAsync(cancellationToken);
                        batch = new Batch<T>(messages);

                        if (cancellationToken.IsCancellationRequested) return;

                        using var timedCts = new CancellationTokenSource(Options.PipelineTimeout);
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timedCts.Token);

                        var activityId = TryGetActivityId(batch);

                        await OnMessageConsumed(batch, Trace.WithActivity(ConsumeActivityName, activityId), cts.Token);
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            Logger.LogError(ex, ex.Message);

                        if (Options.AutoAcknowledge == true && batch != null)
                            await batch.AcknowledgeAsync(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        private async Task<IEnumerable<Message<T>>> ReadBatchAsync(CancellationToken cancellationToken)
        {
            var batchWait = TimeSpan.FromMilliseconds(5);
            var count = Options.BatchSize;

            using var batchCancellationToken = new CancellationTokenSource(batchWait);
            using var combined = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, batchCancellationToken.Token);

            var messages = new List<Message<T>>(Options.BatchSize);
            while (!combined.IsCancellationRequested && count-- > 0)
            {
                messages.Add(await ChannelReader.ReadAsync(cancellationToken));
            }

            return messages;
        }

        /// <summary>
        ///     Occurs when a batch is consumed.
        /// </summary>
        /// <param name="message">The batch consumed.</param>
        /// <param name="cancellationToken">The cancellation token configured to timeout in the configured time.</param>
        /// <param name="tracer">The current activity tracer.</param>
        /// <returns>A task that completes when the handle method completes</returns>
        protected abstract Task OnMessageConsumed(Batch<T> message, Trace.ActivityTracer tracer, CancellationToken cancellationToken);

        /// <inheritDoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping consumer pump: " + GetType().GetFriendlyName());
            return base.StartAsync(cancellationToken);
        }

        private static string TryGetActivityId(Batch<T> batch)
        {
            var activityIds = batch.Select(TryGetActivityId).Where(x => x != null);

            return string.Join("|", activityIds);
        }

        private static string TryGetActivityId(Message<T> message)
        {
            switch (message)
            {
                case ISupportProperties p:
                {
                    return p.Properties.FirstOrDefault(x => x.Key == KnownProperties.ActivityId).Value;
                }
                case ISupportProperties<byte[]> p2:
                {
                    var bytes = p2.Properties.FirstOrDefault(x => x.Key == KnownProperties.ActivityId).Value;
                    return bytes == null ? null : Encoding.UTF8.GetString(bytes);
                }
            }

            return null;
        }
    }
}