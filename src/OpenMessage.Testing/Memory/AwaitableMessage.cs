using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenMessage.Testing.Memory
{
    /// <summary>
    ///     A <see cref="Message{T}" /> that can be awaited as a <see cref="Task" />. The task will complete when the message is acknowledged by the consumer.
    /// </summary>
    internal sealed class AwaitableMessage<T> : Message<T>, ISupportAcknowledgement
    {
        private readonly Message<T> _message;
        private readonly TaskCompletionSource<bool> _messageConsumedTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public AcknowledgementState AcknowledgementState { get; private set; }

        public AwaitableMessage() { }

        public AwaitableMessage(Message<T> message)
        {
            _message = message;
            Value = message.Value;
        }

        public async Task AcknowledgeAsync(bool positivelyAcknowledge = true)
        {
            if (positivelyAcknowledge)
                AcknowledgementState = AcknowledgementState.Acknowledged;
            else
                AcknowledgementState = AcknowledgementState.NegativelyAcknowledged;

            if (_message is ISupportAcknowledgement ack)
                await ack.AcknowledgeAsync(positivelyAcknowledge);

            _messageConsumedTaskCompletionSource.TrySetResult(positivelyAcknowledge);
        }

        public TaskAwaiter<bool> GetAwaiter() => _messageConsumedTaskCompletionSource.Task.GetAwaiter();
    }
}