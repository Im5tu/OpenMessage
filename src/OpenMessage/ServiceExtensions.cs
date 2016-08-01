using Microsoft.Extensions.DependencyInjection;
using System;

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
    }
}
