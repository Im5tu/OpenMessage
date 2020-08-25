using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.AWS.SQS
{
    internal interface IQueueMonitor<T>
    {
        Task<int> GetQueueCountAsync(string consumerId, CancellationToken cancellationToken);
    }
}