using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal interface IKafkaConsumer<TKey, TValue>
    {
        void Start(string consumerId);

        Task<KafkaMessage<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken);

        void Stop();
    }
}