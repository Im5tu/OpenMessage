using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    ///     Defines the basis of a message pump
    /// </summary>
    /// <typeparam name="T">The type produced by the message pump</typeparam>
    public abstract class MessagePump<T> : BackgroundService where T : class
    {
        /// <summary>
        ///     The writable channel to use
        /// </summary>
        protected ChannelWriter<Message<T>> ChannelWriter { get; }

        /// <summary>
        ///     The logger to use
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        protected MessagePump(ChannelWriter<Message<T>> channelWriter, ILogger logger)
        {
            ChannelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritDoc />
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Starting message pump: {GetType().GetFriendlyName()}");

            return base.StartAsync(cancellationToken);
        }

        /// <inheritDoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Stopping message pump: {GetType().GetFriendlyName()}");

            return base.StopAsync(cancellationToken);
        }

        /// <inheritDoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
            await Task.Yield();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Logger.LogError(e, e.Message);
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); // TODO : make this configurable
                    }
                }
            }
        }

        protected abstract Task ConsumeAsync(CancellationToken cancellationToken);
    }
}