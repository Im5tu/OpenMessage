using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenMessage.Providers.Azure.Configuration;
using OpenMessage.Providers.Azure.Conventions;
using OpenMessage.Providers.Azure.Dispatchers;
using OpenMessage.Providers.Azure.Management;
using OpenMessage.Providers.Azure.Observables;
using OpenMessage.Providers.Azure.Serialization;
using System;

namespace OpenMessage
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOpenMessage(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddScoped<IQueueNamingConvention, DefaultNamingConventions>();
            services.TryAddScoped<ISubscriptionNamingConvention, DefaultNamingConventions>();
            services.TryAddScoped<ITopicNamingConvention, DefaultNamingConventions>();

            return services.AddOptions().AddScoped<ISerializationProvider, SerializationProvider>();
        }

        public static IServiceCollection AddQueueObservable<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddBroker<T>();

            return services.AddQueue<T>().AddScoped<IObservable<T>, QueueObservable<T>>();
        }                                                                  
        public static IServiceCollection AddQueueObservable<T>(this IServiceCollection services, Action<T> callback)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return services.AddQueueObservable<T>().AddObserver(callback);
        } 

        public static IServiceCollection AddSubscriptionObservable<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddBroker<T>();

            return services.AddSubscription<T>().AddScoped<IObservable<T>, SubscriptionObservable<T>>();
        }
        public static IServiceCollection AddSubscriptionObservable<T>(this IServiceCollection services, Action<T> callback)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return services.AddSubscriptionObservable<T>().AddObserver(callback);
        }

        public static IServiceCollection AddQueueDispatcher<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddQueue<T>().AddScoped<IDispatcher<T>, QueueDispatcher<T>>();
        }

        public static IServiceCollection AddTopicDispatcher<T>(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTopic<T>().AddScoped<IDispatcher<T>, TopicDispatcher<T>>();
        }

        private static IServiceCollection AddQueue<T>(this IServiceCollection services)
        {
            return services.AddBaseServices<T>().AddScoped<IQueueFactory<T>, QueueFactory<T>>();
        }
        private static IServiceCollection AddTopic<T>(this IServiceCollection services)
        {
            return services.AddBaseServices<T>().AddScoped<ITopicFactory<T>, TopicFactory<T>>();
        }
        private static IServiceCollection AddSubscription<T>(this IServiceCollection services)
        {
            return services.AddBaseServices<T>().AddTopic<T>().AddScoped<ISubscriptionFactory<T>, SubscriptionFactory<T>>();
        }
        private static IServiceCollection AddBaseServices<T>(this IServiceCollection services)
        {
            services.TryAddScoped<IConfigureOptions<OpenMessageAzureProviderOptions<T>>, OpenMessageAzureProviderOptionsConfigurator<T>>();
            services.TryAddScoped<INamespaceManager<T>, NamespaceManager<T>>();

            return services;
        }
    }
}
