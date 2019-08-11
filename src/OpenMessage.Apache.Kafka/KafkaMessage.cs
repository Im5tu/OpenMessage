using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal sealed class KafkaMessage<TKey, TValue> : Message<TValue>, ISupportAcknowledgement, ISupportIdentification<TKey>, ISupportProperties
    {
        private readonly Action _postiveAcknowledgeAction;
        private AcknowledgementState _acknowledgementState = AcknowledgementState.NotAcknowledged;

        /// <inheritdoc />
        AcknowledgementState ISupportAcknowledgement.AcknowledgementState => _acknowledgementState;

        /// <inheritdoc />
        Task ISupportAcknowledgement.AcknowledgeAsync(bool positivelyAcknowledge)
        {
            if (_acknowledgementState != AcknowledgementState.NotAcknowledged)
                return Task.CompletedTask;

            if (positivelyAcknowledge)
            {
                _postiveAcknowledgeAction?.Invoke();
                _acknowledgementState = AcknowledgementState.Acknowledged;
            }
            else
            {
                _acknowledgementState = AcknowledgementState.NegativelyAcknowledged;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public TKey Id { get; internal set; }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> Properties { get; internal set; } = Enumerable.Empty<KeyValuePair<string, string>>();

        internal KafkaMessage(Action postiveAcknowledgeAction)
        {
            _postiveAcknowledgeAction = postiveAcknowledgeAction ?? throw new ArgumentNullException(nameof(postiveAcknowledgeAction));
        }
    }
}