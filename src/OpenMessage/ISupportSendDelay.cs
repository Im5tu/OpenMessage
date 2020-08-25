using System;

namespace OpenMessage
{
    /// <summary>
    ///     Indicates the message supports a send delay
    /// </summary>
    public interface ISupportSendDelay
    {
        /// <summary>
        ///     How long to delay the send of the message. The value is limited by the maximum value allowed by the messaging protocol
        /// </summary>
        public TimeSpan SendDelay { get; set; }
    }
}
