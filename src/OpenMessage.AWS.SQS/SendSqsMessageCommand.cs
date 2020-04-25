using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace OpenMessage.AWS.SQS
{
    internal class SendSqsMessageCommand
    {
        internal string? QueueUrl { get; set; }

        internal SendMessageBatchRequestEntry? Message { get; set; }

        internal TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        internal string? ServiceUrl { get; set; }

        internal string? RegionEndpoint { get; set; }

        internal string LookupKey => _lookupKey ??= $"{QueueUrl ?? string.Empty}|{ServiceUrl ?? string.Empty}|{RegionEndpoint ?? string.Empty}";

        private string? _lookupKey;
    }
}