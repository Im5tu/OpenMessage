using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessagePump<T> : MessagePump<T>
    {
        private readonly ISqsConsumer<T> _sqsConsumer;
        private readonly string _consumerId;

        public SqsMessagePump(ChannelWriter<Message<T>> channelWriter, ILogger logger, ISqsConsumer<T> sqsConsumer, string consumerId)
            : base(channelWriter, logger)
        {
            _sqsConsumer = sqsConsumer ?? throw new ArgumentNullException(nameof(sqsConsumer));
            _consumerId = consumerId ?? throw new ArgumentNullException(nameof(consumerId));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _sqsConsumer.Initialize(_consumerId);
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await _sqsConsumer.ConsumeAsync();
                    foreach (var message in messages)
                        await ChannelWriter.WriteAsync(message, cancellationToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}