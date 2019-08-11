namespace OpenMessage.Serialisation
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
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>An instance of T</returns>
        T From<T>(string data, string contentType);

        /// <summary>
        ///     Deserializes from the specified byte array to the desired T
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <param name="contentType">The content type contained in the data</param>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>An instance of T</returns>
        T From<T>(byte[] data, string contentType);
    }
}