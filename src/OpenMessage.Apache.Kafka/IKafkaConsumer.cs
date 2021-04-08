using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Apache.Kafka
{
    internal interface IKafkaConsumer<TKey, TValue> where TKey : class where TValue : class
    {
        Task<KafkaMessage<TKey, TValue>?> ConsumeAsync(CancellationToken cancellationToken);
        void Start(string consumerId);

        void Stop();
    }
}