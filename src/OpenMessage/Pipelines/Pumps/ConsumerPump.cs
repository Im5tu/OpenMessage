using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Pipelines.Builders;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    ///     The base type for providing a consumer of the internal messaging channel
    /// </summary>
    /// <typeparam name="T">The type that is contained in the message</typeparam>
    public class ConsumerPump<T> : BackgroundService
    {
        private readonly ChannelReader<Message<T>> _channelReader;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsumerPump<T>> _logger;
        private readonly IOptionsMonitor<PipelineOptions<T>> _options;
        private IPipelineBuilder<T> _pipelineBuilder;

        /// <summary>
        ///     ctor
        /// </summary>
        public ConsumerPump(ChannelReader<Message<T>> channelReader,
            IPipelineBuilder<T> pipelineBuilder,
            IServiceProvider serviceProvider,
            ILogger<ConsumerPump<T>> logger,
            IOptionsMonitor<PipelineOptions<T>> options)
        {
            _channelReader = channelReader ?? throw new ArgumentNullException(nameof(channelReader));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
                var pipeline = _pipelineBuilder.Build();

                while (!cancellationToken.IsCancellationRequested && !_channelReader.Completion.IsCompleted)
                {
                    try
                    {
                        var message = await _channelReader.ReadAsync(cancellationToken);

                        if (_options.CurrentValue.PipelineType == PipelineType.Serial)
                        {
                            await pipeline(message, cancellationToken, new MessageContext(_serviceProvider));
                        }
                        else
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await pipeline(message, cancellationToken, new MessageContext(_serviceProvider));
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, e.Message);
                                }
                            }, cancellationToken);
                        }
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

        /// <inheritDoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping consumer pump: " + GetType().GetFriendlyName());
            return base.StopAsync(cancellationToken);
        }
    }
}