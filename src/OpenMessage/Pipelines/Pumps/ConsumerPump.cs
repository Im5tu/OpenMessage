using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    ///     The base type for providing a consumer of the internal messaging channel
    /// </summary>
    /// <typeparam name="T">The type that is contained in the message</typeparam>
    public class ConsumerPump<T> : BackgroundService
    {
        private static readonly string ConsumeActivityName = "OpenMessage.Consumer.Process";
        private readonly ParallelPipelineInitiator<T> _parallelPipelineInitiator;
        private readonly SerialPipelineInitiator<T> _serialPipelineInitiator;
        private readonly ChannelReader<Message<T>> _channelReader;
        private readonly PipelineDelegate.SingleMiddleware<T> _pipeline;
        private readonly IOptionsMonitor<PipelineOptions<T>> _optionsMonitor;
        private readonly ILogger<ConsumerPump<T>> _logger;

        /// <summary>
        ///     ctor
        /// </summary>
        public ConsumerPump(ChannelReader<Message<T>> channelReader,
            IPipelineBuilder<T> pipelineBuilder,
            IOptionsMonitor<PipelineOptions<T>> optionsMonitor,
            ILogger<ConsumerPump<T>> logger,
            ParallelPipelineInitiator<T> parallelPipelineInitiator,
            SerialPipelineInitiator<T> serialPipelineInitiator)
        {
            _parallelPipelineInitiator = parallelPipelineInitiator;
            _serialPipelineInitiator = serialPipelineInitiator;
            _channelReader = channelReader ?? throw new ArgumentNullException(nameof(channelReader));
            _pipeline = pipelineBuilder?.Build() ?? throw new ArgumentNullException(nameof(pipelineBuilder));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritDoc />
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting consumer pump: " + GetType().GetFriendlyName());
            return base.StartAsync(cancellationToken);
        }

        /// <inheritDoc />
        protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && !_channelReader.Completion.IsCompleted)
                {
                    Message<T> msg = null;
                    try
                    {
                        msg = await _channelReader.ReadAsync(cancellationToken);
                        using var timedCts = new CancellationTokenSource(_optionsMonitor.CurrentValue.PipelineTimeout);
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timedCts.Token);

                        _ = TryGetActivityId(msg, out var activityId);

                        var tracer = Trace.WithActivity(ConsumeActivityName, activityId);

                        var initiator = GetPipelineInitiator();

                        await initiator.Initiate(tracer, _pipeline, msg, cts.Token);
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            _logger.LogError(ex, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private IPipelineInitiator<T> GetPipelineInitiator()
        {
            return _optionsMonitor.CurrentValue.PipelineType switch
            {
                PipelineType.Serial => (IPipelineInitiator<T> ) _serialPipelineInitiator,
                PipelineType.Parallel => (IPipelineInitiator<T>) _parallelPipelineInitiator,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritDoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping consumer pump: " + GetType().GetFriendlyName());
            return base.StopAsync(cancellationToken);
        }

        private static bool TryGetActivityId(Message<T> message, out string activityId)
        {
            activityId = null;

            switch (message)
            {
                case ISupportProperties p:
                {
                    foreach (var prop in p.Properties)
                        if (prop.Key == KnownProperties.ActivityId)
                        {
                            activityId = prop.Value;
                            return true;
                        }
                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        if (prop.Key == KnownProperties.ActivityId)
                        {
                            activityId = Encoding.UTF8.GetString(prop.Value);
                            return true;
                        }
                    break;
                }
            }

            return false;
        }
    }
}