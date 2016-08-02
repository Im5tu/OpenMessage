using Jil;
using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Serializer.Jil;
using System;

namespace OpenMessage
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     Adds a JIL serializer to OpenMessage.
        /// </summary>
        public static IServiceCollection AddJilSerializer(this IServiceCollection services, Options options = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTransient<ISerializer, JilSerializer>().AddSingleton(options ?? new Options());
        }
    }
}
