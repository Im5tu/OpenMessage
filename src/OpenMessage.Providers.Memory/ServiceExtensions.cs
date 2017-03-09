using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Providers.Azure;
using System;

namespace OpenMessage
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMemoryChannel<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddScoped(typeof(MemoryChannel<>), typeof(MemoryChannel<>));

            return services.AddBroker<T>().AddScoped<IDispatcher<T>, MemoryChannel<T>>().AddScoped<IObservable<T>, MemoryChannel<T>>();
        }
    }
}
