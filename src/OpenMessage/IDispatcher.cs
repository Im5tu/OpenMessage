using System;
using System.Threading.Tasks;

namespace OpenMessage
{
    public interface IDispatcher<T>
    {
        Task DispatchAsync(T entity, TimeSpan scheduleIn);
    }
}
