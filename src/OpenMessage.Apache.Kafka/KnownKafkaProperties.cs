namespace OpenMessage.Apache.Kafka
{
    /// <summary>
    ///     Properties that are automatically added to a consumed message
    /// </summary>
    public static class KnownKafkaProperties
    {
        /// <summary>
        ///     The offset the message is located at
        /// </summary>
        public static readonly string Offset = nameof(Offset);

        /// <summary>
        ///     The partition the message is located on
        /// </summary>
        public static readonly string Partition = nameof(Partition);

        /// <summary>
        ///     The timestamp of the message on the partition
        /// </summary>
        public static readonly string Timestamp = nameof(Timestamp);

        /// <summary>
        ///     The topic the message was consumed from
        /// </summary>
        public static readonly string Topic = nameof(Topic);
    }
}