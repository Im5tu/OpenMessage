using System;

namespace OpenMessage.Providers.Azure.Conventions
{
    internal sealed class DefaultNamingConventions : IQueueNamingConvention, ISubscriptionNamingConvention, ITopicNamingConvention
    {
        string IQueueNamingConvention.GenerateName<T>() => $"{typeof(T).Namespace}.{typeof(T).GetFriendlyName()}".AsAzureSafeString();

        string ISubscriptionNamingConvention.GenerateName<T>() => Environment.MachineName.AsAzureSafeString();

        string ITopicNamingConvention.GenerateName<T>() => $"{typeof(T).Namespace}.{typeof(T).GetFriendlyName()}".AsAzureSafeString();
    }
}
