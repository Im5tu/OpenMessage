namespace OpenMessage.Providers.Azure.Conventions
{
    public interface IQueueNamingConvention
    {
        string GenerateName<T>();
    }
}
