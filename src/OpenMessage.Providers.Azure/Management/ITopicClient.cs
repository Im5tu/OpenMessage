using System;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Management
{
    internal interface ITopicClient<T> : IDisposable
    {
        Task SendAsync(T entity, TimeSpan scheduleIn);
    }
}
