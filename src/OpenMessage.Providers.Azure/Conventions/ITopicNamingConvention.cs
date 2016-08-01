namespace OpenMessage.Providers.Azure.Conventions
{
    public interface ITopicNamingConvention
    {
        string GenerateName<T>();
    }
}
