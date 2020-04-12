using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines.Pumps;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessagePump<T> : MessagePump<T>
    {
        private readonly string _consumerId;
        private readonly IQueueMonitor<T> _queueMonitor;
        private readonly IOptionsMonitor<SQSConsumerOptions> _sqsOptions;
        private readonly IServiceProvider _services;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _consumerCheckTask;
        private List<ISqsConsumer<T>> _consumers = new List<ISqsConsumer<T>>();

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

        protected override async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            try
            {
                List<ISqsConsumer<T>> consumers;

                lock(_consumers)
                    consumers = _consumers.ToList();

                var messages = await Task.WhenAll(consumers.Select(x => x.ConsumeAsync(cancellationToken)));
                foreach (var message in messages.SelectMany(x => x))
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
            catch (OperationCanceledException) { }
        }

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
                _consumers.Add(consumer);
                Logger.LogInformation("Initialized new '{0}' consumer. Current consumer count: {1}. Queue Length: {2}", TypeCache<T>.FriendlyName, _consumers.Count, queueLength);
            }
        }

        private void RemoveConsumer()
        {
            lock (_consumers)
            {
                if (_consumers.Count == 0)
                    return;

                _consumers.RemoveAt(_consumers.Count - 1);
                Logger.LogInformation("Removed '{0}' consumer. Current consumer count: {1}", TypeCache<T>.FriendlyName, _consumers.Count);
            }
        }
    }
}
