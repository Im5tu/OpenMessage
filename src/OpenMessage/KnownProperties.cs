namespace OpenMessage
{
    /// <summary>
    ///     A list of known properties on a message
    /// </summary>
    public static class KnownProperties
    {
        /// <summary>
        ///     The type the entity has been serialized as
        /// </summary>
        public static readonly string ContentType = nameof(ContentType);

        /// <summary>
        ///     The id of the activity that triggered the message
        /// </summary>
        public static readonly string ActivityId = nameof(ActivityId);

        /// <summary>
        ///     The type of the value that has been serialized
        /// </summary>
        public static readonly string ValueTypeName = nameof(ValueTypeName);

        /// <summary>
        ///     The type of key that has been serialized
        /// </summary>
        public static readonly string KeyTypeName = nameof(KeyTypeName);
    }
}