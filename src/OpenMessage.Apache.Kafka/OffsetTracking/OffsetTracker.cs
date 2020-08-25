using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenMessage.Apache.Kafka.OffsetTracking
{
    internal sealed class OffsetTracker
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<long, bool>> _partitionOffsetMap = new ConcurrentDictionary<int, ConcurrentDictionary<long, bool>>();

        public void AddOffset(in int partition, in long offset)
        {
            GetOrAddPartition(partition).TryAdd(offset, false);
        }

        public void AckOffset(in int partition, in long offset)
        {
            GetOrAddPartition(partition).TryUpdate(offset, true, false);
        }

        public void Clear()
        {
            _partitionOffsetMap.Clear();
        }

        public IEnumerable<AckedOffset> GetAcknowledgedOffsets()
        {
            // For each partition
            foreach (var partitionMap in _partitionOffsetMap)
            {
                long latestContiguousAckedOffset = -1;
                foreach (var offset in partitionMap.Value.ToArray().OrderBy(kvp => kvp.Key))
                {
                    if (!offset.Value)
                        break;

                    latestContiguousAckedOffset = offset.Key;
                }

                // If -1, the lowest numbered tracked offset hasn't been acked, so nothing to
                // commit for this partition
                if (latestContiguousAckedOffset >= 0)
                    yield return new AckedOffset(partitionMap.Key, latestContiguousAckedOffset);
            }
        }

        public void PruneCommitted(AckedOffset committedOffset)
        {
            if (!_partitionOffsetMap.TryGetValue(committedOffset.Partition, out var trackedOffsets))
                return;

            // remove everything with an earlier offset value
            foreach (var key in trackedOffsets.Keys.Where(k => k <= committedOffset.Offset))
                trackedOffsets.TryRemove(key, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ConcurrentDictionary<long, bool> GetOrAddPartition(in int partition)
        {
            return _partitionOffsetMap.GetOrAdd(partition, _ => new ConcurrentDictionary<long, bool>());
        }
    }
}