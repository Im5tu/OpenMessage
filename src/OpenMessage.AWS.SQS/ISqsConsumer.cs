using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenMessage.AWS.SQS
{
    internal interface ISqsConsumer<T>
    {
        void Initialize(string consumerId);

        Task<List<SqsMessage<T>>> ConsumeAsync();
    }
}