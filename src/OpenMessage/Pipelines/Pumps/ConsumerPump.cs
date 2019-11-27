using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ChannelReader<Message<T>> _channelReader;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PipelineDelegate.SingleMiddleware<T> _pipeline;
        private readonly ILogger<ConsumerPump<T>> _logger;

        /// <summary>
        ///     ctor
        /// </summary>
        public ConsumerPump(ChannelReader<Message<T>> channelReader,
            IPipelineBuilder<T> pipelineBuilder,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ConsumerPump<T>> logger)
        {
            _channelReader = channelReader ?? throw new ArgumentNullException(nameof(channelReader));
            _serviceScopeFactory = serviceScopeFactory;
            _pipeline = pipelineBuilder?.Build() ?? throw new ArgumentNullException(nameof(pipelineBuilder));
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
                    try
                    {
                        var message = await _channelReader.ReadAsync(cancellationToken);

                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            await _pipeline(message, cancellationToken, new MessageContext(scope.ServiceProvider));
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