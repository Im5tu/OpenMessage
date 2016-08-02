using System;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Management
{
    internal interface IQueueClient<T> : IDisposable
    {
        void RegisterCallback(Action<T> callback);
        Task SendAsync(T entity, TimeSpan scheduleIn);
    }
}
