using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Dispatching
{
    public abstract class DispatcherBase<T> : IDispatcher<T>, IDisposable
    {
        private bool _isDisposed = false;
        private readonly IDispatchInterceptor<T>[] _dispatchInterceptors;

        public DispatcherBase(IEnumerable<IDispatchInterceptor<T>> dispatchInterceptors)
        {
            if (dispatchInterceptors == null)
                throw new ArgumentNullException(nameof(dispatchInterceptors));

            _dispatchInterceptors = dispatchInterceptors.ToArray();
        }

        public Task DispatchAsync(T entity, TimeSpan scheduleIn)
        {
            ThrowIfDisposed();

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (scheduleIn < TimeSpan.Zero)
                throw new ArgumentNullException(nameof(scheduleIn));

            if (_dispatchInterceptors.Length > 0
                    && _dispatchInterceptors.Any(interceptor => interceptor.Intercept(entity)))
            {
                var errorTask = new TaskCompletionSource<T>();
                errorTask.SetException(new Exception("One or more interceptors blocked the dispatching of the message"));
                return errorTask.Task;
            }

            return OnDispatchMessageAsync(entity, scheduleIn);
        }

        protected abstract Task OnDispatchMessageAsync(T entity, TimeSpan scheduleIn);

        public void Dispose()
        {
            ThrowIfDisposed();

            _isDisposed = true;

            foreach (var disposableDispatchInterceptor in _dispatchInterceptors.OfType<IDisposable>())
                disposableDispatchInterceptor.Dispose();

            OnDispose();
        }
        protected virtual void OnDispose() { }
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
