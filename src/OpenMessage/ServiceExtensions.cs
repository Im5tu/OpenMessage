using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OpenMessage
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     Adds a message broker for the specified type.
        /// </summary>
        public static IServiceCollection TryAddBroker<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddScoped<IBroker<T>, MessageBroker<T>>();
            return services;
        }

        /// <summary>
        ///     Creates an observer from the specified action.
        /// </summary>
        public static IServiceCollection AddObserver<T>(this IServiceCollection services, Action<T> action)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return services.AddScoped<IObserver<T>>(sp => new ActionObserver<T>(action));
        }
    }
}
