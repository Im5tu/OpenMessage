using System;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class TopicFactory<T> : ITopicFactory<T>
    {
        public ITopicClient<T> Create()
        {
            throw new NotImplementedException();
        }
    }
}
