using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Pipelines.Builders;
using Polly.Registry;
using System;

namespace OpenMessage.Polly
{
    /// <summary>
    ///     Extensions for configuring Polly as middleware
    /// </summary>
    public static class PollyExtensions
    {
        /// <summary>
        ///     Adds a readonly policy registry for polly
        /// </summary>
        public static IServiceCollection AddPolly(this IServiceCollection services, Action<IPolicyRegistry<string>> configuration)
        {
            var registry = new PolicyRegistry();
            configuration?.Invoke(registry);

            return services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);
        }

        /// <summary>
        ///     Adds a Polly IAsyncPolicy as middleware.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder</param>
        /// <param name="policy">The name of the policy to use</param>
        /// <typeparam name="T">The underlying type for the pipeline</typeparam>
        /// <returns>The modified pipeline builder</returns>
        public static IPipelineBuilder<T> UsePolly<T>(this IPipelineBuilder<T> pipelineBuilder, string policy) => UsePolly(pipelineBuilder, options => options.PolicyName = policy);

        /// <summary>
        ///     Adds a Polly IAsyncPolicy as middleware.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder</param>
        /// <param name="options">Option configuration</param>
        /// <typeparam name="T">The underlying type for the pipeline</typeparam>
        /// <returns>The modified pipeline builder</returns>
        public static IPipelineBuilder<T> UsePolly<T>(this IPipelineBuilder<T> pipelineBuilder, Action<PollyMiddlewareOptions<T>> options)
        {
            pipelineBuilder.Services.TryAddSingleton<PollyMiddleware<T>>();
            pipelineBuilder.Services.Configure(options);

            return pipelineBuilder.Use<PollyMiddleware<T>>();
        }
    }
}