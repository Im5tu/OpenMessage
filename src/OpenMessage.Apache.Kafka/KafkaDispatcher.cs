using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Apache.Kafka.Configuration;
using OpenMessage.Serialization;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal sealed class KafkaDispatcher<T> : KafkaClient, IDispatcher<T>
    {
        private readonly byte[] _contentType;
        private readonly IOptionsMonitor<KafkaOptions<T>> _options;
        private readonly IProducer<byte[], byte[]> _producer;
        private readonly ISerializer _serializer;

        public KafkaDispatcher(ILogger<KafkaDispatcher<T>> logger, IOptionsMonitor<KafkaOptions<T>> options, ISerializer serializer)
            : base(logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _contentType = Encoding.UTF8.GetBytes(_serializer.ContentType);
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _producer = new ProducerBuilder<byte[], byte[]>(options.CurrentValue.KafkaConfiguration).SetErrorHandler(Kafka_OnError)
                                                                                                    .SetLogHandler(Kafka_OnLog)
                                                                                                    .SetStatisticsHandler(Kafka_OnStatistics)
                                                                                                    .Build();
        }

        public Task DispatchAsync(T entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            return DispatchAsync(new Message<T>
            {
                Value = entity
            }, cancellationToken);
        }

        public async Task DispatchAsync(Message<T> message, CancellationToken cancellationToken)
        {
            if (message is null)
                Throw.ArgumentNullException(nameof(message));

            cancellationToken.ThrowIfCancellationRequested();

            var headers = CreateHeadersFromExisting(message);
            var key = CreateKeyForMessage(message);

            var msg = new Message<byte[], byte[]>
            {
                Key = key,
                Headers = headers,
                Value = _serializer.AsBytes(message.Value)
            };

            await _producer.ProduceAsync(_options.CurrentValue.TopicName, msg);
        }

        private Headers CreateHeadersFromExisting(Message<T> message)
        {
            Headers HeadersIncludingDefaults(Headers h, bool contentType = false, bool valueType = false)
            {
                if (!contentType)
                    h.Add(KnownProperties.ContentType, _contentType);

                if (!valueType)
                    h.Add(KnownProperties.ValueTypeName, Encoding.UTF8.GetBytes(message.Value.GetType().AssemblyQualifiedName));

                if (Activity.Current is {})
                    h.Add(KnownProperties.ActivityId, Encoding.UTF8.GetBytes(Activity.Current.Id));

                return h;
            }

            // TODO :: Add support for activity id
            var headers = new Headers();

            switch (message)
            {
                case ISupportProperties p:
                {
                    foreach (var prop in p.Properties)
                        headers.Add(prop.Key, Encoding.UTF8.GetBytes(prop.Value));

                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        headers.Add(prop.Key, prop.Value);

                    break;
                }
                case ISupportProperties<byte[], byte[]> p3:
                {
                    foreach (var prop in p3.Properties)
                        headers.Add(Encoding.UTF8.GetString(prop.Key), prop.Value);

                    break;
                }
            }

            if (headers.Count == 0)
                return HeadersIncludingDefaults(headers);

            bool hasContentType = false,
                hasValueType = false;

            foreach (var header in headers)
            {
                if (header.Key == KnownProperties.ContentType)
                    hasContentType = true;

                if (header.Key == KnownProperties.ValueTypeName)
                    hasValueType = true;
            }

            return HeadersIncludingDefaults(headers, hasContentType, hasValueType);
        }

        private byte[] CreateKeyForMessage(Message<T> message)
        {
            return message switch
            {
                ISupportIdentification<byte[]> mi => mi.Id,
                ISupportIdentification mi2 => _serializer.AsBytes(mi2.Id),
                _ => _serializer.AsBytes(Guid.NewGuid())
            };
        }
    }
}