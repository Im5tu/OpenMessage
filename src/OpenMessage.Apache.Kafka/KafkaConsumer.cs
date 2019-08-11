using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Apache.Kafka.Configuration;
using OpenMessage.Apache.Kafka.OffsetTracking;
using OpenMessage.Serialisation;

namespace OpenMessage.Apache.Kafka
{
    internal sealed class KafkaConsumer<TKey, TValue> : KafkaClient, IKafkaConsumer<TKey, TValue>
    {
        private static readonly string DefaultContentType = "application/json";
        private static readonly string TimestampFormat = "o";

        private readonly OffsetTracker _offsetTracker = new OffsetTracker();
        private readonly IDeserializationProvider _deserializationProvider;
        private readonly IOptionsMonitor<KafkaOptions> _options;
        private IConsumer<byte[], byte[]> _consumer;
        private Task _trackAcknowledgedTask;
        private string _topicName;

        public KafkaConsumer(ILogger<KafkaConsumer<TKey, TValue>> logger,
            IDeserializationProvider deserializationProvider,
            IOptionsMonitor<KafkaOptions> options)
            : base(logger)
        {
            _deserializationProvider = deserializationProvider ?? throw new ArgumentNullException(nameof(deserializationProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Start(string consumerId)
        {
            var options = _options.Get(consumerId);
            var offsetTrackerInterval = options.KafkaConfiguration.TryGetValue("auto.commit.interval.ms", out var val) && double.TryParse(val, out var ms)
                ? TimeSpan.FromMilliseconds(ms)
                : TimeSpan.FromSeconds(1);
            _topicName = options.TopicName;
            _consumer = new ConsumerBuilder<byte[], byte[]>(options.KafkaConfiguration)
                .SetErrorHandler(Kafka_OnError)
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
                while (_consumer != null)
                {
                    try
                    {
                        foreach (var offset in _offsetTracker.GetAcknowledgedOffsets())
                        {
                            if (loggerEnabled)
                                Logger.LogInformation($"Committing '{_topicName}' on partition '{offset.Partition}' to offset '{offset.Offset}'");

                            _consumer.StoreOffset(new TopicPartitionOffset(new TopicPartition(_topicName, new Partition(offset.Partition)), new Offset(offset.Offset)));
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

        public Task<KafkaMessage<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    var message = _consumer.Consume(cancellationToken);
                    if (message == null || message.IsPartitionEOF)
                    {
                        Logger.LogInformation(message == null
                            ? "No message received from consumer."
                            : $"End of partition reached. Topic: {message.Topic} Partition: {message.Partition} Offset: {message.Offset}");

                        return null;
                    }

                    var messageProperties = ParseMessageHeaders(message, out var contentType);
                    if (string.IsNullOrWhiteSpace(contentType)) contentType = DefaultContentType;

                    var key = _deserializationProvider.From<TKey>(message.Key, contentType);
                    var value = _deserializationProvider.From<TValue>(message.Value, contentType);

                    _offsetTracker.AddOffset(message.Partition, message.Offset);

                    return new KafkaMessage<TKey, TValue>(() =>
                        _offsetTracker.AckOffset(message.Partition, message.Offset))
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

        public void Stop()
        {
            _consumer.Unsubscribe();
            _offsetTracker.Clear();
        }

        private IEnumerable<KeyValuePair<string, string>> ParseMessageHeaders(ConsumeResult<byte[], byte[]> message, out string contentType)
        {
            contentType = DefaultContentType;

            if (message.Headers == null)
                return Enumerable.Empty<KeyValuePair<string, string>>();

            var headers = new List<KeyValuePair<string, string>>(message.Headers.Count + 3)
            {
                new KeyValuePair<string, string>(KnownKafkaProperties.Offset, message.Offset.Value.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(KnownKafkaProperties.Partition, message.Partition.Value.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(KnownKafkaProperties.Timestamp, message.Timestamp.UtcDateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(KnownKafkaProperties.Topic, _topicName)
            };

            foreach (var header in message.Headers)
            {
                var value = Encoding.UTF8.GetString(header.GetValueBytes());
                headers.Add(new KeyValuePair<string, string>(header.Key, value));
                if (header.Key == KnownProperties.ContentType)
                    contentType = value;
            }

            return headers;
        }

        #region Logging

        private void OnOffsetsCommitted(IConsumer<byte[], byte[]> consumer, CommittedOffsets e)
        {
            if (!Logger.IsEnabled(LogLevel.Trace) || e?.Offsets == null)
                return;

            foreach (var offset in e.Offsets)
                Logger.LogTrace(
                    $"Offset Committed On Topic {offset.Topic}. Partition: {offset.Partition.Value} Offset: {offset.Offset}");
        }

        private void OnPartitionsRevoked(IConsumer<byte[], byte[]> consumer, List<TopicPartitionOffset> topicPartitions)
        {
            if (topicPartitions == null || !Logger.IsEnabled(LogLevel.Information))
                return;

            var topics = string.Join(" | ",
                topicPartitions.Select(x => $"Topic: {x.Topic} Partition: {x.Partition.Value}"));
            Logger.LogInformation($"Rebalancing: {topics}");
        }

        private void OnPartitionsAssigned(IConsumer<byte[], byte[]> consumer, List<TopicPartition> topicPartitions)
        {
            if (topicPartitions == null || !Logger.IsEnabled(LogLevel.Information))
                return;

            var topics = string.Join(" | ",
                topicPartitions.Select(x => $"Topic: {x.Topic} Partition: {x.Partition.Value}"));
            Logger.LogInformation($"Assigning: {topics}");
        }

        #endregion
    }
}