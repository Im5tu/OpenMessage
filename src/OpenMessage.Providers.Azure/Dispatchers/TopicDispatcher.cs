using OpenMessage.Providers.Azure.Management;
using System;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Dispatchers
{
    internal sealed class TopicDispatcher<T> : IDispatcher<T>
    {
        private readonly ITopicClient<T> _client;

        public TopicDispatcher(ITopicFactory<T> topicFactory)
        {
            if (topicFactory == null)
                throw new ArgumentNullException(nameof(topicFactory));

            _client = topicFactory.Create();
        }

        public Task DispatchAsync(T entity, TimeSpan scheduleIn) => _client.SendAsync(entity, scheduleIn);
    }
}
