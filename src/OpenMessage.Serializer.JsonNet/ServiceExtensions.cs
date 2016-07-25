using Microsoft.Extensions.DependencyInjection;
using System;

namespace OpenMessage.Serializer.JsonNet
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddJsonNetSerializer(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTransient<ISerializer, JsonNetSerializer>();
        }
    }
}
