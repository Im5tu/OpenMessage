using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure
{
    internal sealed class MemoryChannel<T> : ManagedObservable<T>, IDispatcher<T>
    {
        private readonly IDispatchInterceptor<T>[] _dispatchInterceptors;

        public MemoryChannel(ILogger<ManagedObservable<T>> logger, IEnumerable<IDispatchInterceptor<T>> dispatchInterceptors) 
            : base(logger)
        {
            if (dispatchInterceptors == null)
                throw new ArgumentNullException(nameof(dispatchInterceptors));

            _dispatchInterceptors = dispatchInterceptors.ToArray();
        }

        public Task DispatchAsync(T entity, TimeSpan scheduleIn)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (scheduleIn < TimeSpan.Zero)
                throw new ArgumentException("You cannot schedule a message in the past.");

            if (_dispatchInterceptors.Length > 0 
                && _dispatchInterceptors.Any(interceptor => !interceptor.Intercept(entity)))
            {
                var errorTask = new TaskCompletionSource<T>();
                errorTask.SetException(new Exception("One or more interceptors blocked the dispatching of the message"));
                return errorTask.Task;
            }

            return Task.Run(async () =>
            {
                if (scheduleIn > TimeSpan.Zero)
                    await Task.Delay(scheduleIn);

                Notify(entity);
            });
        }
    }
}
