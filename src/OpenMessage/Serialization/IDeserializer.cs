using System;
using System.Collections.Generic;

namespace OpenMessage.Serialization
{
    /// <summary>
    ///     An instance of a deserializer
    /// </summary>
    public interface IDeserializer
    {
        /// <summary>
        ///     Determines which content types are supported by this deserializer
        /// </summary>
        IEnumerable<string> SupportedContentTypes { get; }

        /// <summary>
        ///     Deserializes the data to a given T
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="data">The data to convert from</param>
        /// <param name="messageType">The original type of the message</param>
        /// <returns>An instance of T</returns>
        T From<T>(string data, Type messageType);

        /// <summary>
        ///     Deserializes the data to a given T
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="data">The data to convert from</param>
        /// <param name="messageType">The original type of the message</param>
        /// <returns>An instance of T</returns>
        T From<T>(byte[] data, Type messageType);
    }
}