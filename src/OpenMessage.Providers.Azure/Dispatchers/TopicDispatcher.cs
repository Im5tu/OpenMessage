using OpenMessage.Providers.Azure.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Dispatchers
{
    internal sealed class TopicDispatcher<T> : IDispatcher<T>
    {
        private readonly ITopicClient<T> _client;
        private readonly IDispatchInterceptor<T>[] _dispatchInterceptors;

        public TopicDispatcher(ITopicFactory<T> topicFactory,
            IEnumerable<IDispatchInterceptor<T>> dispatchInterceptors)
        {
            if (topicFactory == null)
                throw new ArgumentNullException(nameof(topicFactory));

            if (dispatchInterceptors == null)
                throw new ArgumentNullException(nameof(dispatchInterceptors));

            _client = topicFactory.Create();
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

            return _client.SendAsync(entity, scheduleIn);
        }
    }
}
