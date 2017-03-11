using Microsoft.Extensions.DependencyInjection;
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

            return services.AddBroker<T>().AddScoped<IDispatcher<T>, MemoryChannel<T>>().AddScoped<IObservable<T>, MemoryChannel<T>>();
        }
    }
}
