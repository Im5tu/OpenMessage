using System.Collections.Generic;

namespace OpenMessage
{
    /// <summary>
    ///     Indicates that the message supports properties
    /// </summary>
    public interface ISupportProperties<TKey, TValue>
    {
        /// <summary>
        ///     The properties associated with the message
        /// </summary>
        IEnumerable<KeyValuePair<TKey, TValue>> Properties { get; }
    }
}