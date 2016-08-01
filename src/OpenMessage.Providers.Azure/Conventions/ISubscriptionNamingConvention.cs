namespace OpenMessage.Providers.Azure.Conventions
{
    public interface ISubscriptionNamingConvention
    {
        string GenerateName<T>();
    }
}
