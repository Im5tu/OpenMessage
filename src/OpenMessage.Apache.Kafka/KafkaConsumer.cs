using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Apache.Kafka.Configuration;
using OpenMessage.Apache.Kafka.OffsetTracking;
using OpenMessage.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal sealed class KafkaConsumer<TKey, TValue> : KafkaClient, IKafkaConsumer<TKey, TValue>
    {
        private static readonly string TimestampFormat = "o";
        private readonly Action<KafkaMessage<TKey, TValue>> _acknowledgementAction;
        private readonly IDeserializationProvider _deserializationProvider;

        private readonly OffsetTracker _offsetTracker = new OffsetTracker();
        private readonly IOptionsMonitor<KafkaOptions> _options;
        private IConsumer<byte[], byte[]> _consumer;
        private string _topicName;
        private Task _trackAcknowledgedTask;

        public KafkaConsumer(ILogger<KafkaConsumer<TKey, TValue>> logger, IDeserializationProvider deserializationProvider, IOptionsMonitor<KafkaOptions> options)
            : base(logger)
        {
            _deserializationProvider = deserializationProvider ?? throw new ArgumentNullException(nameof(deserializationProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _acknowledgementAction = msg => _offsetTracker.AckOffset(msg.Partition, msg.Offset);
        }

        public Task<KafkaMessage<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (consumeResult is null || consumeResult.IsPartitionEOF)
                    {
                        Logger.LogInformation(consumeResult is null ? "No message received from consumer." : $"End of partition reached. Topic: {consumeResult.Topic} Partition: {consumeResult.Partition} Offset: {consumeResult.Offset}");

                        return null;
                    }

                    var messageProperties = ParseMessageHeaders(consumeResult, out var contentType, out var messageType);

                    if (string.IsNullOrWhiteSpace(contentType))
                        contentType = ContentTypes.Json;

                    var key = _deserializationProvider.From<TKey>(consumeResult.Message.Key, contentType, TypeCache<TKey>.AssemblyQualifiedName);
                    var value = _deserializationProvider.From<TValue>(consumeResult.Message.Value, contentType, messageType);

                    _offsetTracker.AddOffset(consumeResult.Partition, consumeResult.Offset);

                    return new KafkaMessage<TKey, TValue>(_acknowledgementAction, consumeResult.Partition.Value, consumeResult.Offset.Value)
                    {
                        Id = key,
                        Properties = messageProperties,
                        Value = value
                    };
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            });
        }

        public void Start(string consumerId)
        {
            var options = _options.Get(consumerId);
            var offsetTrackerInterval = options.KafkaConfiguration.TryGetValue("auto.commit.interval.ms", out var val) && double.TryParse(val, out var ms) ? TimeSpan.FromMilliseconds(ms) : TimeSpan.FromSeconds(1);
            _topicName = options.TopicName;

            _consumer = new ConsumerBuilder<byte[], byte[]>(options.KafkaConfiguration).SetErrorHandler(Kafka_OnError)
                                                                                       .SetLogHandler(Kafka_OnLog)
                                                                                       .SetOffsetsCommittedHandler(OnOffsetsCommitted)
                                                                                       .SetPartitionsAssignedHandler(OnPartitionsAssigned)
                                                                                       .SetPartitionsRevokedHandler(OnPartitionsRevoked)
                                                                                       .SetStatisticsHandler(Kafka_OnStatistics)
                                                                                       .Build();
            _consumer.Subscribe(_topicName);

            _trackAcknowledgedTask = Task.Run(async () =>
            {
                var loggerEnabled = Logger.IsEnabled(LogLevel.Information);

                while (_consumer is {})
                {
                    try
                    {
                        foreach (var offset in _offsetTracker.GetAcknowledgedOffsets())
                        {
                            if (loggerEnabled)
                                Logger.LogInformation($"Committing '{_topicName}' on partition '{offset.Partition}' to offset '{offset.Offset}'");

                            _consumer.StoreOffset(new TopicPartitionOffset(new TopicPartition(_topicName, new Partition(offset.Partition)), new Offset(offset.Offset + 1)));
                            _offsetTracker.PruneCommitted(offset);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                    }

                    await Task.Delay(offsetTrackerInterval);
                }
            });
        }

        public void Stop()
        {
            _consumer.Unsubscribe();
            _offsetTracker.Clear();
        }

        private IEnumerable<KeyValuePair<string, string>> ParseMessageHeaders(ConsumeResult<byte[], byte[]> consumeResult, out string contentType, out string messageType)
        {
            contentType = ContentTypes.Json;
            messageType = null;

            if (consumeResult.Message.Headers is null)
                return Enumerable.Empty<KeyValuePair<string, string>>();

            var headers = new List<KeyValuePair<string, string>>(consumeResult.Message.Headers.Count + 3)
            {
                new KeyValuePair<string, string>(KnownKafkaProperties.Offset, consumeResult.Offset.Value.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(KnownKafkaProperties.Partition, consumeResult.Partition.Value.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(KnownKafkaProperties.Timestamp, consumeResult.Message.Timestamp.UtcDateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(KnownKafkaProperties.Topic, _topicName)
            };

            foreach (var header in consumeResult.Message.Headers)
            {
                var value = Encoding.UTF8.GetString(header.GetValueBytes());
                headers.Add(new KeyValuePair<string, string>(header.Key, value));

                if (header.Key == KnownProperties.ContentType)
                    contentType = value;

                if (header.Key == KnownProperties.ValueTypeName)
                    messageType = value;
            }

            return headers;
        }

        #region Logging

        private void OnOffsetsCommitted(IConsumer<byte[], byte[]> consumer, CommittedOffsets e)
        {
            if (!Logger.IsEnabled(LogLevel.Trace) || e?.Offsets is null)
                return;

            foreach (var offset in e.Offsets)
                Logger.LogTrace($"Offset Committed On Topic {offset.Topic}. Partition: {offset.Partition.Value} Offset: {offset.Offset}");
        }

        private void OnPartitionsRevoked(IConsumer<byte[], byte[]> consumer, List<TopicPartitionOffset> topicPartitions)
        {
            if (topicPartitions is null || !Logger.IsEnabled(LogLevel.Information))
                return;

            var topics = string.Join(" | ", topicPartitions.Select(x => $"Topic: {x.Topic} Partition: {x.Partition.Value}"));
            Logger.LogInformation($"Rebalancing: {topics}");
        }

        private void OnPartitionsAssigned(IConsumer<byte[], byte[]> consumer, List<TopicPartition> topicPartitions)
        {
            if (topicPartitions is null || !Logger.IsEnabled(LogLevel.Information))
                return;

            var topics = string.Join(" | ", topicPartitions.Select(x => $"Topic: {x.Topic} Partition: {x.Partition.Value}"));
            Logger.LogInformation($"Assigning: {topics}");
        }

        #endregion
    }
}