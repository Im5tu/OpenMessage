using System;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class QueueFactory<T> : IQueueFactory<T>
    {
        public IQueueClient<T> Create()
        {
            throw new NotImplementedException();
        }
    }
}
