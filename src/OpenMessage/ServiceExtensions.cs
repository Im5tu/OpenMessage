using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace OpenMessage
{
    public static class ServiceExtensions
    {
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

        /// <summary>
        ///     Adds a broker for the given type to the service collection specified.
        /// </summary>
        public static IServiceCollection AddBroker<T>(this IServiceCollection services)
        {
            if (services.Any(service => service.ServiceType == typeof(IBroker) && service.ServiceType == typeof(MessageBroker<T>)))
                return services;

            return services.AddScoped<IBroker, MessageBroker<T>>();
        }
    }
}
