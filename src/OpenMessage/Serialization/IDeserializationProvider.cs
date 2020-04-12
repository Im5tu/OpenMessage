namespace OpenMessage.Serialization
{
    /// <summary>
    ///     Negotiates the deserializer to use based on a content type
    /// </summary>
    public interface IDeserializationProvider
    {
        /// <summary>
        ///     Deserializes from the specified string to the desired T
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <param name="contentType">The content type contained in data</param>
        /// <param name="type">The type contained within data</param>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>An instance of T</returns>
        T From<T>(string data, string contentType, string type);

        /// <summary>
        ///     Deserializes from the specified byte array to the desired T
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <param name="contentType">The content type contained in the data</param>
        /// <param name="type">The type contained within data</param>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>An instance of T</returns>
        T From<T>(byte[] data, string contentType, string type);
    }
}