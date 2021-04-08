using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines.Pumps;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessagePump<T> : MessagePump<T> where T : class
    {
        private readonly string _consumerId;
        private readonly IQueueMonitor<T> _queueMonitor;
        private readonly IOptionsMonitor<SQSConsumerOptions> _sqsOptions;
        private readonly IServiceProvider _services;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _consumerCheckTask;
        private List<(Task tsk, CancellationTokenSource cts)> _consumers = new List<(Task tsk, CancellationTokenSource cts)>();

        public SqsMessagePump(ChannelWriter<Message<T>> channelWriter,
            ILogger<SqsMessagePump<T>> logger,
            IQueueMonitor<T> queueMonitor,
            IServiceScopeFactory serviceScopeFactory,
            IOptionsMonitor<SQSConsumerOptions> sqsOptions,
            string consumerId)
            : base(channelWriter, logger)
        {
            _queueMonitor = queueMonitor ?? throw new ArgumentNullException(nameof(queueMonitor));
            _sqsOptions = sqsOptions ?? throw new ArgumentNullException(nameof(sqsOptions));
            if (serviceScopeFactory == null)
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            _services = serviceScopeFactory.CreateScope().ServiceProvider;
            _consumerId = consumerId ?? throw new ArgumentNullException(nameof(consumerId));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _consumerCheckTask = Task.Run(async () =>
            {
                await Task.Delay(100);
                var token = _cancellationTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // This is hacky POC
                        var count = await _queueMonitor.GetQueueCountAsync(_consumerId, token);

                        lock (_consumers)
                        {
                            const int targetCountPerConsumer = 50;
                            var options = _sqsOptions.Get(_consumerId);
                            if (_consumers.Count == 0)
                            {
                                // This is the startup essentially
                                var newConsumerCount = Math.Min(count == 0 ? options.MinimumConsumerCount : Math.Max(count / targetCountPerConsumer, options.MinimumConsumerCount), options.MaximumConsumerCount);
                                for (var i = 0; i < newConsumerCount; i++)
                                {
                                    InitialiseConsumer(count, cancellationToken);
                                }
                            }
                            else if (count >= 0)
                            {
                                var maxCapacity = _consumers.Count * targetCountPerConsumer;
                                if (count > (maxCapacity + targetCountPerConsumer * 3) && _consumers.Count < options.MaximumConsumerCount)
                                {
                                    InitialiseConsumer(count, cancellationToken);
                                }
                                else if (count < (maxCapacity / 2) && _consumers.Count - 1 >= options.MinimumConsumerCount)
                                {
                                    RemoveConsumer();
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error occurred while running '{TypeCache<T>.FriendlyName}' {nameof(SqsMessagePump<T>)}. {ex.Message}");
                    }
                    finally
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            await Task.Delay(5000, cancellationToken);
                    }
                }
            });

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
        }

        protected override Task ConsumeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task HandleMissingQueueAsync<TException>(TException exception, CancellationToken cancellationToken)
            where TException : Exception
        {
            Logger.LogError(exception, $"Queue for type '{TypeCache<T>.FriendlyName}' does not exist. Retrying in 15 seconds.");
            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }

        private void InitialiseConsumer(int queueLength, CancellationToken cancellationToken)
        {
            lock (_consumers)
            {
                var consumer = _services.GetRequiredService<ISqsConsumer<T>>();
                consumer.Initialize(_consumerId, cancellationToken);
                var cts = new CancellationTokenSource();
                var ct = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                var consumerTask = RunConsumerTask(consumer, ct.Token);
                _consumers.Add((consumerTask, cts));
                Logger.LogInformation("Initialized new '{0}' consumer. Current consumer count: {1}. Queue Length: {2}", TypeCache<T>.FriendlyName, _consumers.Count, queueLength);
            }
        }

        private void RemoveConsumer()
        {
            lock (_consumers)
            {
                if (_consumers.Count == 0)
                    return;

                var index = _consumers.Count - 1;
                var tskGroup = _consumers[index];
                _consumers.RemoveAt(index);
                tskGroup.cts.Cancel(false);
                Logger.LogInformation("Removed '{0}' consumer. Current consumer count: {1}", TypeCache<T>.FriendlyName, _consumers.Count);
            }
        }

        private async Task RunConsumerTask(ISqsConsumer<T> consumer, CancellationToken cancellationToken)
        {
            var writer = ChannelWriter;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await consumer.ConsumeAsync(cancellationToken);
                    foreach (var message in messages)
                        if (!writer.TryWrite(message))
                            await writer.WriteAsync(message, cancellationToken);
                }
                catch (QueueDoesNotExistException queueException)
                {
                    await HandleMissingQueueAsync(queueException, cancellationToken);
                }
                catch (AmazonSQSException sqsException) when (sqsException.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
                {
                    await HandleMissingQueueAsync(sqsException, cancellationToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Logger.LogError(e, e.Message);
                    throw;
                }
            }
        }
    }
}