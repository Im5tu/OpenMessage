namespace OpenMessage.Providers.Azure.Management
{
    internal interface ITopicFactory<T>
    {
        ITopicClient<T> Create();
    }
}
