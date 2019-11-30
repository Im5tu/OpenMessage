using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenMessage;
using OpenMessage.DI;
using OpenMessage.Handlers;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Endpoints;
using OpenMessage.Pipelines.Middleware;
using OpenMessage.Pipelines.Pumps;
using OpenMessage.Serialisation;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Adds OpenMessage to the specified host
    /// </summary>
    public static class OpenMessageExtensions
    {
        /// <summary>
        ///     Adds OpenMessage
        /// </summary>
        /// <param name="hostBuilder">The host configuration</param>
        /// <param name="builder">The OpenMessage builder - use this to configure consumers and dispatchers</param>
        /// <returns>The modified host builder</returns>
        public static IHostBuilder ConfigureMessaging(this IHostBuilder hostBuilder, Action<IMessagingBuilder> builder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                builder?.Invoke(new MessagingBuilder(context, services));
                services.AddSerialization();
                services.TryAddSingleton<ISerializer, DefaultSerializer>();
                services.TryAddSingleton<IDeserializer, DefaultDeserializer>();
                services.AddSingleton(typeof(AutoAcknowledgeMiddleware<>));
                services.AddSingleton(typeof(ServiceScopeMiddleware<>));
                services.AddSingleton(typeof(TimeoutMiddleware<>));
                services.AddSingleton(typeof(TraceMiddleware<>));
                services.AddSingleton(typeof(LoggerScopeMiddleware<>));
                services.AddSingleton(typeof(BatchPipelineEndpoint<>));
                services.AddScoped(typeof(HandlerPipelineEndpoint<>));
                services.AddScoped(typeof(BatchHandlerPipelineEndpoint<>));
            });
        }

        #region IMessagingBuilder

        /// <summary>
        ///     Adds the specified handler
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <typeparam name="THandler">The type of the handler</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureHandler<T, THandler>(this IMessagingBuilder messagingBuilder)
            where THandler : class, IHandler<T>
        {
            messagingBuilder.Services.AddScoped<IHandler<T>, THandler>();
            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the specified batch handler
        /// </summary>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <typeparam name="TBatchHandler">The type of the batch handler to add</typeparam>
        /// <param name="messagingBuilder">The OpenMessageBuilder</param>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureBatchHandler<T, TBatchHandler>(this IMessagingBuilder messagingBuilder)
            where TBatchHandler : class, IBatchHandler<T>
        {
            messagingBuilder.Services.AddScoped<IBatchHandler<T>, TBatchHandler>();
            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the specified handler
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="instance">The handler to use</param>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureHandler<T>(this IMessagingBuilder messagingBuilder, IHandler<T> instance)
        {
            messagingBuilder.Services.AddSingleton<IHandler<T>>(instance);
            return messagingBuilder;
        }

        /// <summary>
        ///     Adds the specified handler
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="action">The implementation of the handler</param>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureHandler<T>(this IMessagingBuilder messagingBuilder, Action<Message<T>, CancellationToken> action)
        {
            return messagingBuilder.ConfigureHandler<T>(new ActionHandler<T>(action));
        }

        /// <summary>
        ///     Adds the specified handler
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="action">The implementation of the handler</param>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureHandler<T>(this IMessagingBuilder messagingBuilder, Action<Message<T>> action)
        {
            return messagingBuilder.ConfigureHandler<T>(new ActionHandler<T>(action));
        }

        /// <summary>
        ///     Adds the specified handler
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="action">The implementation of the handler</param>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureHandler<T>(this IMessagingBuilder messagingBuilder, Func<Message<T>, CancellationToken, Task> action)
        {
            return messagingBuilder.ConfigureHandler<T>(new ActionHandler<T>(action));
        }

        /// <summary>
        ///     Adds the specified handler
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="action">The implementation of the handler</param>
        /// <typeparam name="T">The type that the handler handles</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureHandler<T>(this IMessagingBuilder messagingBuilder, Func<Message<T>, Task> action)
        {
            return messagingBuilder.ConfigureHandler<T>(new ActionHandler<T>(action));
        }

        public static IPipelineBuilder<T> ConfigurePipeline<T>(this IMessagingBuilder messagingBuilder, Action<PipelineOptions<T>> configurator = null) => ConfigurePipeline<T>(messagingBuilder, (_, options) => configurator?.Invoke(options));

        public static IPipelineBuilder<T> ConfigurePipeline<T>(this IMessagingBuilder messagingBuilder, Action<HostBuilderContext, PipelineOptions<T>> configurator)
        {
            if (configurator != null)
            {
                messagingBuilder.Services.Configure<PipelineOptions<T>>(options => { configurator(messagingBuilder.Context, options); });
            }

            messagingBuilder.Services.AddSingleton<IPostConfigureOptions<PipelineOptions<T>>, PipelineOptionsPostConfigurationProvider<T>>();

            return new PipelineBuilder<T>(messagingBuilder);
        }

        /// <summary>
        ///     Configures all handlers that can be found in the specified assemblies
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="assembliesToScan">The assemblies to scan</param>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigureAllHandlers(this IMessagingBuilder messagingBuilder, params Assembly[] assembliesToScan)
        {
            if (assembliesToScan?.Length == 0)
                assembliesToScan = new[] {Assembly.GetEntryAssembly()};

            var handlerTypes = new[] {typeof(IHandler<>), typeof(IBatchHandler<>)};

            IEnumerable<Type> HandlerInterfaceFilter(TypeInfo ti)
            {
                return ti.ImplementedInterfaces.Where(x => x.IsGenericType && handlerTypes.Contains(x.GetGenericTypeDefinition()));
            }

            foreach (var assembly in assembliesToScan)
            {
                var types = assembly.GetTypes()
                                    .Where(x => !x.IsAbstract &&
                                                !x.IsInterface &&
                                                HandlerInterfaceFilter(x.GetTypeInfo())
                                                    .Any());

                foreach (var handlerType in types)
                {
                    var implementedHandlers = HandlerInterfaceFilter(handlerType.GetTypeInfo());

                    foreach (var implementedHandler in implementedHandlers)
                        messagingBuilder.Services.AddScoped(implementedHandler, handlerType);
                }
            }

            return messagingBuilder;
        }

        #endregion

        #region IServiceCollection

        /// <summary>
        ///     Adds the core services required for serialization
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddSerialization(this IServiceCollection services)
        {
            services.TryAddSingleton<IDeserializationProvider, DeserializationProvider>();
            return services;
        }

        /// <summary>
        ///     Adds the specified serializer
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddSerializer<T>(this IServiceCollection services)
            where T : class, ISerializer
        {
            return services.AddSingleton<ISerializer, T>();
        }

        /// <summary>
        ///     Adds the specified deserializer
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddDeserializer<T>(this IServiceCollection services)
            where T : class, IDeserializer
        {
            return services.AddSingleton<IDeserializer, T>();
        }

        /// <summary>
        ///     Adds the background channel if it has not already been added
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <param name="channelCreator">A function that creates a channel</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection TryAddChannel<T>(this IServiceCollection services, Func<IServiceProvider, Channel<T>> channelCreator = null)
        {
            if (channelCreator == null)
                services.TryAddSingleton(sp =>
                {
                    var pipelineOptions = sp.GetRequiredService<IOptionsMonitor<PipelineOptions<T>>>().CurrentValue;

                    return pipelineOptions.UseBoundedChannel.GetValueOrDefault(true)
                        ? Channel.CreateBounded<T>(new BoundedChannelOptions(pipelineOptions.BoundedChannelLimit.GetValueOrDefault(Environment.ProcessorCount * 10)) {SingleReader = true, SingleWriter = false})
                        : Channel.CreateUnbounded<T>(new UnboundedChannelOptions {SingleReader = true, SingleWriter = false});
                });
            else
                services.TryAddSingleton(channelCreator);

            services.TryAddSingleton(sp => sp.GetRequiredService<Channel<T>>().Reader);
            services.TryAddSingleton(sp => sp.GetRequiredService<Channel<T>>().Writer);

            return services;
        }

        /// <summary>
        ///     Adds the background channel if it has not already been added
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <param name="channelCreator">A function that creates a channel</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection TryAddConsumerService<T>(this IServiceCollection services, Func<IServiceProvider, Channel<Message<T>>> channelCreator = null)
        {
            services.TryAddChannel(channelCreator);
            services.TryAddSingleton<IPostConfigureOptions<PipelineOptions<T>>, PipelineOptionsPostConfigurationProvider<T>>();
            services.TryAddSingleton<IPipelineBuilder<T>>(_ => new PipelineBuilder<T>().UseDefaultMiddleware());

            if (!services.Any(x => x.ServiceType == typeof(IHostedService) && x.ImplementationType == typeof(ConsumerPump<T>)))
            {
                services.AddSingleton<IHostedService, ConsumerPump<T>>();
            }

            return services;
        }

        /// <summary>
        ///     Adds the background channel
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <param name="consumerId">The consumer id</param>
        /// <typeparam name="T">The type to handle</typeparam>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddConsumerService<T>(this IServiceCollection services, string consumerId)
            where T : IHostedService
        {
            return services.AddSingleton<IHostedService>(sp => ActivatorUtilities.CreateInstance<T>(sp, consumerId));
        }
        #endregion
    }
}