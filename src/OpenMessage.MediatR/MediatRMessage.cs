using MediatR;

namespace OpenMessage.MediatR
{
    /// <summary>
    /// Ensure that all <see cref="INotificationHandler{TNotification}"/> listen to a notification type of <see cref="MediatRBatch{T}"/> or <see cref="MediatRMessage{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MediatRMessage<T> : INotification
    {
        /// <summary>
        /// The original message
        /// </summary>
        public Message<T> OriginalMessage { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalMessage"></param>
        public MediatRMessage(Message<T> originalMessage)
        {
            OriginalMessage = originalMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static implicit operator MediatRMessage<T>(Message<T> message) => new MediatRMessage<T>(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static implicit operator Message<T>(MediatRMessage<T> message) => message.OriginalMessage;
    }
}