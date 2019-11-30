using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines.Pumps;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessagePump<T> : MessagePump<T>
    {
        private readonly ISqsConsumer<T> _sqsConsumer;
        private readonly string _consumerId;

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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await _sqsConsumer.ConsumeAsync(cancellationToken);
                    if (messages.Count == 0)
                        continue;

                    foreach (var message in messages)
                        await ChannelWriter.WriteAsync(message, cancellationToken);
                }
                catch (QueueDoesNotExistException queueException)
                {
                    await HandleMissingQueue(queueException, cancellationToken);
                }
                catch (AmazonSQSException sqsException) when (sqsException.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
                {
                    await HandleMissingQueue(sqsException, cancellationToken);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }
        }

        private async Task HandleMissingQueue<TException>(TException exception, CancellationToken cancellationToken)
            where TException : Exception
        {
            Logger.LogError(exception, $"Queue for type '{TypeCache<T>.FriendlyName}' does not exist. Retrying in 15 seconds.");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
            }
            finally { } // Doesn't matter if the retry gets cancelled here.
        }
    }
}