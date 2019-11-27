using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenMessage.Memory
{
    /// <summary>
    /// A <see cref="Message{T}"/> that can be awaited as a <see cref="Task"/>. The task will complete when the message is acknowledged by the consumer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AwaitableMessage<T> : Message<T>, ISupportAcknowledgement
    {
        private readonly Message<T> _message;
        private readonly TaskCompletionSource<bool> _messageConsumedTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public AwaitableMessage()
        {
            
        }

        public AwaitableMessage(Message<T> message)
        {
            _message = message;
            Value = message.Value;
        }

        public TaskAwaiter<bool> GetAwaiter() => _messageConsumedTaskCompletionSource.Task.GetAwaiter();

        public AcknowledgementState AcknowledgementState { get; private set; }

        public async Task AcknowledgeAsync(bool positivelyAcknowledge = true)
        {
            _messageConsumedTaskCompletionSource.TrySetResult(positivelyAcknowledge);

            if (_message is ISupportAcknowledgement ack)
            {
                await ack.AcknowledgeAsync(positivelyAcknowledge);
                AcknowledgementState = ack.AcknowledgementState;
                return;
            }

            if (positivelyAcknowledge)
            {
                AcknowledgementState = AcknowledgementState.Acknowledged;
            }
            else
            {
                AcknowledgementState = AcknowledgementState.NegativelyAcknowledged;
            }
        }
    }
}