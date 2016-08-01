using System;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class SubscriptionFactory<T> : ISubscriptionFactory<T>
    {
        public ISubscriptionClient<T> Create()
        {
            throw new NotImplementedException();
        }
    }
}
