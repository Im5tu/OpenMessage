namespace OpenMessage.Providers.Azure.Management
{
    internal interface ISubscriptionFactory<T>
    {
        ISubscriptionClient<T> Create();
    }
}
