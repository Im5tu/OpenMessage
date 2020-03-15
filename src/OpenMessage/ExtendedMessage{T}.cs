using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenMessage
{
    /// <summary>
    /// Like a normal message, but supports properties and identification
    /// </summary>
    public class ExtendedMessage<T> : Message<T>, ISupportProperties, ISupportIdentification
    {
        /// <inheritDoc />
        public IEnumerable<KeyValuePair<string, string>> Properties { get; set; } = Enumerable.Empty<KeyValuePair<string, string>>();

        /// <inheritDoc />
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        ///     Creates a new message
        /// </summary>
        public ExtendedMessage()
        {
        }

        /// <summary>
        ///     Creates a new message with the specified value
        /// </summary>
        public ExtendedMessage(T value)
        {
            Value = value;
        }
    }
}