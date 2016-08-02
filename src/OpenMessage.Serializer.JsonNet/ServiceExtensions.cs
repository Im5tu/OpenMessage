using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Serializer.JsonNet;
using System;

namespace OpenMessage
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     Adds a Newtonsoft.Json serializer to OpenMessage.
        /// </summary>
        public static IServiceCollection AddJsonNetSerializer(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTransient<ISerializer, JsonNetSerializer>();
        }
    }
}
