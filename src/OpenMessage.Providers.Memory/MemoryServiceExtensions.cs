using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Providers.Memory;
using System;

namespace OpenMessage
{
    public static class MemoryServiceExtensions
    {
        public static IServiceCollection AddMemoryChannel<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddScoped<MemoryChannel<T>, MemoryChannel<T>>();

            return services.AddBroker<T>().AddScoped<IDispatcher<T>>(sp => sp.GetRequiredService<MemoryChannel<T>>()).AddScoped<IObservable<T>>(sp => sp.GetRequiredService<MemoryChannel<T>>());
        }
    }
}
