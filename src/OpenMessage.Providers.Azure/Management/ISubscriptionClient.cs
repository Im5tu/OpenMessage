using System;

namespace OpenMessage.Providers.Azure.Management
{
    internal interface ISubscriptionClient<T> : IDisposable
    {
        void RegisterCallback(Action<T> callback);
    }
}
