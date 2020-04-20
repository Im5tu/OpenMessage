using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace OpenMessage.AWS.SQS
{
    internal class SendSqsMessageCommand
    {
        internal string? QueueUrl { get; set; }
        internal SendMessageBatchRequestEntry? Message { get; set; }
        internal TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}