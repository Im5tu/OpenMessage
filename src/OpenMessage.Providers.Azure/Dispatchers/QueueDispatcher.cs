using OpenMessage.Dispatching;
using OpenMessage.Providers.Azure.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Dispatchers
{
    internal sealed class QueueDispatcher<T> : DispatcherBase<T>
    {
        private readonly IQueueClient<T> _client;

        public QueueDispatcher(IQueueFactory<T> queueFactory,
            IEnumerable<IDispatchInterceptor<T>> dispatchInterceptors) : base(dispatchInterceptors)
        {
            if (queueFactory == null)
                throw new ArgumentNullException(nameof(queueFactory));

            _client = queueFactory.Create();
        }

        protected override Task OnDispatchMessageAsync(T entity, TimeSpan scheduleIn) => _client.SendAsync(entity, scheduleIn);

        protected override void OnDispose()
        {
            _client?.Dispose();
        }
    }
}
