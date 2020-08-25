using MediatR;

namespace OpenMessage.MediatR
{
    /// <summary>
    ///     Ensure that all <see cref="INotificationHandler{TNotification}" /> listen to a notification type of <see cref="MediatRBatch{T}" /> or <see cref="MediatRMessage{T}" />
    /// </summary>
    public class MediatRMessage<T> : INotification
    {
        /// <summary>
        ///     The original message
        /// </summary>
        public Message<T> OriginalMessage { get; }

        /// <param name="originalMessage"></param>
        public MediatRMessage(Message<T> originalMessage) => OriginalMessage = originalMessage;

        /// <param name="message"></param>
        public static implicit operator MediatRMessage<T>(Message<T> message) => new MediatRMessage<T>(message);

        /// <param name="message"></param>
        public static implicit operator Message<T>(MediatRMessage<T> message) => message.OriginalMessage;
    }
}