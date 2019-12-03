using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal interface IKafkaConsumer<TKey, TValue>
    {
        Task<KafkaMessage<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken);
        void Start(string consumerId);

        void Stop();
    }
}