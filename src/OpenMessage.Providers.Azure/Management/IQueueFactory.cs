namespace OpenMessage.Providers.Azure.Management
{
    internal interface IQueueFactory<T>
    {
        IQueueClient<T> Create();
    }
}
