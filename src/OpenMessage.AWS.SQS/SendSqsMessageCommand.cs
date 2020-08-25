using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace OpenMessage.AWS.SQS
{
    internal class SendSqsMessageCommand
    {
        private string? _lookupKey;
#if NETCOREAPP3_1
        private OpenMessageEventSource.ValueStopwatch? _stopwatch;
#endif
        private TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        internal string? QueueUrl { get; set; }
        internal SendMessageBatchRequestEntry? Message { get; set; }
        internal string? ServiceUrl { get; set; }
        internal string? RegionEndpoint { get; set; }
        internal string LookupKey => _lookupKey ??= $"{QueueUrl ?? string.Empty}|{ServiceUrl ?? string.Empty}|{RegionEndpoint ?? string.Empty}";

        public SendSqsMessageCommand()
        {
#if NETCOREAPP3_1
            _stopwatch = OpenMessageEventSource.Instance.ProcessMessageDispatchStart();
#endif
        }

        internal void Complete()
        {
            _taskCompletionSource.TrySetResult(true);
            CompleteCore();
        }

        internal void Cancel(CancellationToken ct)
        {
            _taskCompletionSource.TrySetCanceled(ct);
            CompleteCore();
        }

        internal void Exception(Exception ex)
        {
            _taskCompletionSource.TrySetException(ex);
            CompleteCore();
        }

        internal Task WaitForCompletion() => _taskCompletionSource.Task;

        private void CompleteCore()
        {
#if NETCOREAPP3_1
            if (_stopwatch.HasValue)
                OpenMessageEventSource.Instance.ProcessMessageDispatchStop(_stopwatch.Value);
#endif
        }
    }
}