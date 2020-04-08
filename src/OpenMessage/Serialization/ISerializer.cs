namespace OpenMessage.Serialization
{
    /// <summary>
    ///     A serializer for a given entity
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        ///     Content type that's returned, eg: application/json
        /// </summary>
        string ContentType { get; }

        /// <summary>
        ///     Serializes to a byte[]
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entity">The entity</param>
        /// <returns>A byte array with the serialized entity</returns>
        byte[] AsBytes<T>(T entity);

        /// <summary>
        ///     Serializes to a string
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entity">The entity</param>
        /// <returns>A string with the serialized entity</returns>
        string AsString<T>(T entity);
    }
}