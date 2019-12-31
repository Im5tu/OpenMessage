using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsMessage<T> : Message<T>, ISupportAcknowledgement, ISupportIdentification, ISupportProperties
    {
        private readonly Func<SqsMessage<T>, Task> _acknowledgementAction;
#if NETCOREAPP3_1
        private readonly OpenMessageEventSource.ValueStopwatch? _stopwatch;
#endif

        public AcknowledgementState AcknowledgementState { get; private set; }
        public string Id { get; internal set; }
        public IEnumerable<KeyValuePair<string, string>> Properties { get; internal set;  }
        internal string ReceiptHandle { get; set; }
        internal string QueueUrl { get; set; }

        public SqsMessage(Func<SqsMessage<T>, Task> acknowledgementAction)
        {
            _acknowledgementAction = acknowledgementAction;
#if NETCOREAPP3_1
            _stopwatch = OpenMessageEventSource.Instance.ProcessMessageStart();
#endif
        }

        public async Task AcknowledgeAsync(bool positivelyAcknowledge = true, Exception exception = null)
        {
            try
            {
                if (!positivelyAcknowledge)
                {
                    AcknowledgementState = AcknowledgementState.NegativelyAcknowledged;
                    return;
                }

                await _acknowledgementAction(this);
                AcknowledgementState = AcknowledgementState.Acknowledged;
            }
            finally
            {
#if NETCOREAPP3_1
                if (_stopwatch.HasValue)
                    OpenMessageEventSource.Instance.ProcessMessageStop(_stopwatch.Value);
#endif
            }
        }
    }
}