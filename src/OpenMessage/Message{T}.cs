using System.Diagnostics.CodeAnalysis;

namespace OpenMessage
{
    /// <summary>
    ///     Represents a message from another system of a give type
    /// </summary>
    /// <typeparam name="T">The type of the message sent by the counterpart system</typeparam>
    public class Message<T>
    {
        /// <summary>
        ///     The entity sent by the counterpart system
        /// </summary>
        [MaybeNull, AllowNull]
        public T Value { get; set; }

        /// <summary>
        ///     Creates a new message
        /// </summary>
        public Message()
        {
            Value = default;
        }

        /// <summary>
        ///     Creates a new message with the specified value
        /// </summary>
        public Message(T value)
        {
            Value = value;
        }

        /// <summary>
        ///     Implicitly converts the message to the type T
        /// </summary>
        /// <param name="message">The message to convert</param>
        /// <returns>Default if the message is null, otherwise the Value</returns>
        [return: MaybeNull]
        public static implicit operator T(Message<T> message) => message is null ? default : message.Value;
    }
}