using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal sealed class KafkaMessage<TKey, TValue> : Message<TValue>, ISupportAcknowledgement, ISupportIdentification<TKey>, ISupportProperties
    {
        private readonly Action<KafkaMessage<TKey, TValue>> _postiveAcknowledgeAction;
        private AcknowledgementState _acknowledgementState = AcknowledgementState.NotAcknowledged;

        /// <inheritdoc />
        public TKey Id { get; internal set; }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> Properties { get; internal set; } = Enumerable.Empty<KeyValuePair<string, string>>();

        internal long Offset { get; }
        internal int Partition { get; }

        /// <inheritdoc />
        AcknowledgementState ISupportAcknowledgement.AcknowledgementState => _acknowledgementState;

        internal KafkaMessage(Action<KafkaMessage<TKey, TValue>> postiveAcknowledgeAction, int partition, long offset)
        {
            Partition = partition;
            Offset = offset;
            _postiveAcknowledgeAction = postiveAcknowledgeAction ?? throw new ArgumentNullException(nameof(postiveAcknowledgeAction));
        }

        /// <inheritdoc />
        Task ISupportAcknowledgement.AcknowledgeAsync(bool positivelyAcknowledge, Exception exception)
        {
            if (_acknowledgementState != AcknowledgementState.NotAcknowledged)
                return Task.CompletedTask;

            if (positivelyAcknowledge)
            {
                _postiveAcknowledgeAction?.Invoke(this);
                _acknowledgementState = AcknowledgementState.Acknowledged;
            }
            else
            {
                _acknowledgementState = AcknowledgementState.NegativelyAcknowledged;
            }

            return Task.CompletedTask;
        }
    }
}