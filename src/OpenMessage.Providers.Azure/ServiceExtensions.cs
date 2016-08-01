using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Providers.Azure.Serialization;

namespace OpenMessage.Providers.Azure
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOpenMessage(this IServiceCollection services)
        {
            return services.AddTransient<ISerializationProvider, SerializationProvider>();
        }
    }
}
