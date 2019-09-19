using System;
using System.Collections.Generic;
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
using OpenMessage.Serialisation;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Adds OpenMessage to the specified host
    /// </summary>
    public static class OpenMessageExtensions
    {
        private static readonly HashSet<Type> RegisteredConsumers = new HashSet<Type>();

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

        /// <summary>
        ///     Adds the specified pipeline
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="pipelineOptions">Pre-configure the pipeline options</param>
        /// <typeparam name="T">The type the pipeline handles</typeparam>
        /// <typeparam name="TPipeline">The pipeline to use</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigurePipeline<T, TPipeline>(this IMessagingBuilder messagingBuilder, Action<PipelineOptions<T>> pipelineOptions = null)
            where TPipeline : class, IPipeline<T>
        {
            messagingBuilder.Services.TryAddPipeline<T, TPipeline>();
            return messagingBuilder.ConfigurePipelineOptions(pipelineOptions);
        }

        /// <summary>
        ///     Adds the specified pipeline
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="pipelineOptions">Pre-configure the pipeline options</param>
        /// <typeparam name="T">The type the pipeline handles</typeparam>
        /// <typeparam name="TPipeline">The pipeline to use</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigurePipeline<T, TPipeline>(this IMessagingBuilder messagingBuilder, Action<HostBuilderContext, PipelineOptions<T>> pipelineOptions = null)
            where TPipeline : class, IPipeline<T>
        {
            messagingBuilder.Services.TryAddPipeline<T, TPipeline>();
            return messagingBuilder.ConfigurePipelineOptions(pipelineOptions);
        }

        /// <summary>
        ///     Configures options for the specified pipeline
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="configurator">The configuration to apply</param>
        /// <typeparam name="T">The type for the message pipeline, eg: MyEvent</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigurePipelineOptions<T>(this IMessagingBuilder messagingBuilder, Action<PipelineOptions<T>> configurator)
        {
            if (configurator != null)
                messagingBuilder.Services.Configure<PipelineOptions<T>>(configurator);

            return messagingBuilder;
        }

        /// <summary>
        ///     Configures options for the specified pipeline
        /// </summary>
        /// <param name="messagingBuilder">The OpenMessage builder</param>
        /// <param name="configurator">The configuration to apply</param>
        /// <typeparam name="T">The type for the message pipeline, eg: MyEvent</typeparam>
        /// <returns>The OpenMessageBuilder</returns>
        public static IMessagingBuilder ConfigurePipelineOptions<T>(this IMessagingBuilder messagingBuilder, Action<HostBuilderContext, PipelineOptions<T>> configurator)
        {
            if (configurator != null)
                messagingBuilder.Services.Configure<PipelineOptions<T>>(options => { configurator(messagingBuilder.Context, options); });

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
                services.TryAddSingleton(sp => Channel.CreateUnbounded<T>(new UnboundedChannelOptions {SingleReader = true, SingleWriter = false}));
            else
                services.TryAddSingleton(channelCreator);

            services.TryAddSingleton(sp => sp.GetRequiredService<Channel<T>>().Reader);
            services.TryAddSingleton(sp => sp.GetRequiredService<Channel<T>>().Writer);

            return services;
        }

        /// <summary>
        ///     Tries to add the specified pipeline
        /// </summary>
        /// <param name="services">The service collection to modify</param>
        /// <typeparam name="T">The type the pipeline handles</typeparam>
        /// <typeparam name="TPipeline">The type of pipeline</typeparam>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection TryAddPipeline<T, TPipeline>(this IServiceCollection services)
            where TPipeline : class, IPipeline<T>
        {
            services.TryAddSingleton<IPipeline<T>, TPipeline>();
            services.TryAddSingleton<IPostConfigureOptions<PipelineOptions<T>>, PipelineOptionsPostConfigurationProvider<T>>();
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
            services.TryAddChannel(channelCreator).TryAddPipeline<T, SimplePipeline<T>>();

            if (!RegisteredConsumers.Contains(typeof(T)))
            {
                services.AddSingleton(typeof(IHostedService), sp =>
                {
                    var pipelineType = sp.GetRequiredService<IOptionsMonitor<PipelineOptions<T>>>().CurrentValue.PipelineType;
                    return pipelineType switch
                    {
                        PipelineType.Serial => (object) ActivatorUtilities.CreateInstance<SerialConsumerPump<T>>(sp),
                        PipelineType.Parallel => ActivatorUtilities.CreateInstance<ParallelConsumerPump<T>>(sp),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                });
                RegisteredConsumers.Add(typeof(T));
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