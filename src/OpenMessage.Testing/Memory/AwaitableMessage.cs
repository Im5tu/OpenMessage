using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenMessage.Testing.Memory
{
    /// <summary>
    ///     A <see cref="Message{T}" /> that can be awaited as a <see cref="Task" />. The task will complete when the message is acknowledged by the consumer.
    /// </summary>
    internal sealed class AwaitableMessage<T> : Message<T>, ISupportAcknowledgement, ISupportIdentification
    {
        private readonly Message<T> _message;
        private readonly TaskCompletionSource<bool> _messageConsumedTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public AcknowledgementState AcknowledgementState { get; private set; }
        public string Id { get; set; }

        public AwaitableMessage([CallerMemberName] string id = null)
        {
            Id = id;
        }

        public AwaitableMessage(Message<T> message, [CallerMemberName]string id = null) : this(id)
        {
            _message = message;
            Value = message.Value;
        }

        public async Task AcknowledgeAsync(bool positivelyAcknowledge = true, Exception exception = null)
        {
            if (positivelyAcknowledge)
                AcknowledgementState = AcknowledgementState.Acknowledged;
            else
                AcknowledgementState = AcknowledgementState.NegativelyAcknowledged;

            if (_message is ISupportAcknowledgement ack)
                await ack.AcknowledgeAsync(positivelyAcknowledge, exception);

            if (exception == null)
                _messageConsumedTaskCompletionSource.TrySetResult(positivelyAcknowledge);
            else
                _messageConsumedTaskCompletionSource.TrySetException(exception);
        }

        public TaskAwaiter<bool> GetAwaiter() => _messageConsumedTaskCompletionSource.Task.GetAwaiter();
    }
}