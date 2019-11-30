using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Pipelines.Endpoints;
using OpenMessage.Pipelines.Middleware;
using System;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    ///     A builder for configuring a pipeline
    /// </summary>
    public interface IPipelineBuilder<T>
    {
        /// <summary>
        ///     The service collection this pipeline is being built on
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        ///     Funnels a pipeline into a batched pipeline
        /// </summary>
        /// <returns>A builder to configure an <see cref="IBatchPipelineBuilder{T}" /></returns>
        IBatchPipelineBuilder<T> Batch();

        /// <summary>
        ///     Builds the pipeline
        /// </summary>
        /// <returns></returns>
        PipelineDelegate.SingleMiddleware<T> Build();

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        /// <param name="endpoint"></param>
        void Run(Func<PipelineDelegate.SingleMiddleware<T>> endpoint);

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        /// <param name="constructorParameters">Parameters to be passed into the middleware constructor</param>
        void Run<TPipelineEndpoint>(params object[] constructorParameters)
            where TPipelineEndpoint : IPipelineEndpoint<T>;

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        IPipelineBuilder<T> Use(Func<PipelineDelegate.SingleMiddleware<T>, PipelineDelegate.SingleMiddleware<T>> middleware);

        /// <summary>
        ///     Adds an <see cref="IMiddleware{T}" /> type to the pipeline
        /// </summary>
        /// <param name="constructorParameters">Parameters to be passed into the middleware constructor</param>
        /// <returns></returns>
        IPipelineBuilder<T> Use<TMiddleware>(params object[] constructorParameters)
            where TMiddleware : IMiddleware<T>;
    }
}