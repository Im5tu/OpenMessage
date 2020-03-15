using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    ///     The base type for providing a consumer of the internal messaging channel
    /// </summary>
    /// <typeparam name="T">The type that is contained in the message</typeparam>
    public class ConsumerPump<T> : BackgroundService
    {
        private readonly ChannelReader<Message<T>> _channelReader;
        private readonly ILogger<ConsumerPump<T>> _logger;
        private readonly IOptionsMonitor<PipelineOptions<T>> _options;
        private readonly IServiceProvider _serviceProvider;
        private PipelineDelegate.SingleMiddleware<T> _pipeline;

        /// <summary>
        ///     ctor
        /// </summary>
        public ConsumerPump(ChannelReader<Message<T>> channelReader, IPipelineBuilder<T> pipelineBuilder, IServiceProvider serviceProvider, ILogger<ConsumerPump<T>> logger, IOptionsMonitor<PipelineOptions<T>> options)
        {
            _channelReader = channelReader ?? throw new ArgumentNullException(nameof(channelReader));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _pipeline = (pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder))).Build();
        }

        /// <inheritDoc />
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting consumer pump: {GetType().GetFriendlyName()}");

            return base.StartAsync(cancellationToken);
        }

        /// <inheritDoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping consumer pump: {GetType().GetFriendlyName()}");

            return base.StopAsync(cancellationToken);
        }

        /// <inheritDoc />
        protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
            await Task.Yield();

            try
            {
                while (!cancellationToken.IsCancellationRequested && !_channelReader.Completion.IsCompleted)
                    try
                    {
                        var message = await _channelReader.ReadAsync(cancellationToken);

                        // TODO :: We don't need to check this every time, we just change the implementation when the options changes and use a field to represent the option we want to use.
                        if (_options.CurrentValue.PipelineType == PipelineType.Serial)
                            await _pipeline(message, cancellationToken, new MessageContext(_serviceProvider));
                        else
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await _pipeline(message, cancellationToken, new MessageContext(_serviceProvider));
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, e.Message);
                                }
                            }, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            _logger.LogError(ex, ex.Message);
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}