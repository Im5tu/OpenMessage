using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines.Pumps;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka.HostedServices
{
    internal sealed class KafkaMessagePump<TKey, TValue> : MessagePump<TValue>
    {
        private readonly string _consumerId;
        private readonly IKafkaConsumer<TKey, TValue> _kafkaConsumer;

        public KafkaMessagePump(ChannelWriter<Message<TValue>> channelWriter, ILogger<KafkaMessagePump<TKey, TValue>> logger, IKafkaConsumer<TKey, TValue> kafkaConsumer, string consumerId)
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _kafkaConsumer.Stop();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    var kafkaMessage = await _kafkaConsumer.ConsumeAsync(cancellationToken);

                    if (kafkaMessage is {})
                        await ChannelWriter.WriteAsync(kafkaMessage, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        Logger.LogError(ex, ex.Message);
                }
        }
    }
}