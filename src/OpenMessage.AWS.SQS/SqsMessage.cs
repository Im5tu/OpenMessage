using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessage<T> : Message<T>, ISupportAcknowledgement, ISupportIdentification, ISupportProperties
    {
        private readonly Func<SqsMessage<T>, Task> _acknowledgementAction;
        public AcknowledgementState AcknowledgementState { get; private set; }
        public string Id { get; internal set; }
        public IEnumerable<KeyValuePair<string, string>> Properties { get; internal set;  }
        internal string ReceiptHandle { get; set; }
        internal string QueueUrl { get; set; }

        public async Task AcknowledgeAsync(bool positivelyAcknowledge = true, Exception exception = null)
        {
            if (!positivelyAcknowledge)
            {
                AcknowledgementState = AcknowledgementState.NegativelyAcknowledged;
                return;
            }

            await _acknowledgementAction(this);
            AcknowledgementState = AcknowledgementState.Acknowledged;
        }

        public SqsMessage(Func<SqsMessage<T>, Task> acknowledgementAction)
        {
            _acknowledgementAction = acknowledgementAction;
        }
    }
}