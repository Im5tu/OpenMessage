using System.Collections;
using System.Collections.Generic;
using MediatR;

namespace OpenMessage.MediatR
{
    /// <summary>
    /// Ensure that all <see cref="INotificationHandler{TNotification}"/> listen to a notification type of <see cref="MediatRBatch{T}"/> or <see cref="MediatRMessage{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MediatRBatch<T> : INotification, IReadOnlyCollection<Message<T>>
    {
        private readonly IReadOnlyCollection<Message<T>> _messages;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        public MediatRBatch(IReadOnlyCollection<Message<T>> messages)
        {
            _messages = messages;
        }

        /// <inheritdoc />
        public IEnumerator<Message<T>> GetEnumerator() => _messages.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _messages).GetEnumerator();

        /// <inheritdoc />
        public int Count => _messages.Count;
    }
}
