using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    ///     Defines the basis of a message pump
    /// </summary>
    /// <typeparam name="T">The type produced by the message pump</typeparam>
    public abstract class MessagePump<T> : BackgroundService
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
        protected MessagePump(ChannelWriter<Message<T>> channelWriter,
            ILogger logger)
        {
            ChannelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritDoc />
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting message pump: " + GetType().GetFriendlyName());
            return base.StartAsync(cancellationToken);
        }

        /// <inheritDoc />
        protected abstract override Task ExecuteAsync(CancellationToken cancellationToken);

        /// <inheritDoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping message pump: " + GetType().GetFriendlyName());
            return base.StopAsync(cancellationToken);
        }
    }
}