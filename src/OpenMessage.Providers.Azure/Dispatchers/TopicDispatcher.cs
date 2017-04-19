using OpenMessage.Dispatching;
using OpenMessage.Providers.Azure.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Dispatchers
{
    internal sealed class TopicDispatcher<T> : DispatcherBase<T>
    {
        private readonly ITopicClient<T> _client;

        public TopicDispatcher(ITopicFactory<T> topicFactory,
            IEnumerable<IDispatchInterceptor<T>> dispatchInterceptors) : base(dispatchInterceptors)
        {
            if (topicFactory == null)
                throw new ArgumentNullException(nameof(topicFactory));

            _client = topicFactory.Create();
        }

        protected override Task OnDispatchMessageAsync(T entity, TimeSpan scheduleIn) => _client.SendAsync(entity, scheduleIn);

        protected override void OnDispose()
        {
            _client?.Dispose();
        }
    }
}
