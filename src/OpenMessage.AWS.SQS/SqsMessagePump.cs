using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines.Pumps;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessagePump<T> : MessagePump<T>
    {
        private readonly string _consumerId;
        private readonly ISqsConsumer<T> _sqsConsumer;

        public SqsMessagePump(ChannelWriter<Message<T>> channelWriter, ILogger<SqsMessagePump<T>> logger, ISqsConsumer<T> sqsConsumer, string consumerId)
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

        protected override async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            try
            {
                var messages = await _sqsConsumer.ConsumeAsync(cancellationToken);

                if (messages.Count == 0)
                    return;

                foreach (var message in messages)
                    await ChannelWriter.WriteAsync(message, cancellationToken);
            }
            catch (QueueDoesNotExistException queueException)
            {
                await HandleMissingQueueAsync(queueException, cancellationToken);
            }
            catch (AmazonSQSException sqsException) when (sqsException.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
            {
                await HandleMissingQueueAsync(sqsException, cancellationToken);
            }
            catch (TaskCanceledException) { }
        }

        private async Task HandleMissingQueueAsync<TException>(TException exception, CancellationToken cancellationToken)
            where TException : Exception
        {
            Logger.LogError(exception, $"Queue for type '{TypeCache<T>.FriendlyName}' does not exist. Retrying in 15 seconds.");
            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }
    }
}