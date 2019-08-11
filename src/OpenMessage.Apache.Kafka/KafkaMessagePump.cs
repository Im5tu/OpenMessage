using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines;

namespace OpenMessage.Apache.Kafka.HostedServices
{
    internal sealed class KafkaMessagePump<TKey, TValue> : MessagePump<TValue>
    {
        private readonly IKafkaConsumer<TKey, TValue> _kafkaConsumer;
        private readonly string _consumerId;

        public KafkaMessagePump(ChannelWriter<Message<TValue>> channelWriter,
            ILogger<KafkaMessagePump<TKey, TValue>> logger,
            IKafkaConsumer<TKey, TValue> kafkaConsumer,
            string consumerId)
            : base(channelWriter, logger)
        {
            _kafkaConsumer = kafkaConsumer ?? throw new ArgumentNullException(nameof(kafkaConsumer));
            _consumerId = consumerId ?? throw new ArgumentNullException(nameof(consumerId));
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _kafkaConsumer.Start(_consumerId);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var kafkaMessage = await _kafkaConsumer.ConsumeAsync(stoppingToken);
                    if (kafkaMessage != null)
                        await ChannelWriter.WriteAsync(kafkaMessage, stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _kafkaConsumer.Stop();
        }
    }
}