namespace OpenMessage.Apache.Kafka.OffsetTracking
{
    internal readonly struct AckedOffset
    {
        internal readonly int Partition;
        internal readonly long Offset;

        internal AckedOffset(int partition, long offset)
        {
            Partition = partition;
            Offset = offset;
        }
    }
}