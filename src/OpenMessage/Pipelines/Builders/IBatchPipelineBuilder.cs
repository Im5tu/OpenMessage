using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Pipelines.Endpoints;
using OpenMessage.Pipelines.Middleware;
using System;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    ///     A builder for configuring a batched pipeline
    /// </summary>
    public interface IBatchPipelineBuilder<T>
    {
        /// <summary>
        ///     The service collection this pipeline is being built on
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        ///     Builds the pipeline
        /// </summary>
        /// <returns></returns>
        PipelineDelegate.BatchMiddleware<T> Build();

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        /// <param name="endpoint"></param>
        void Run(Func<PipelineDelegate.BatchMiddleware<T>> endpoint);

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        /// <param name="constructorParameters">Parameters to be passed into the middleware constructor</param>
        void Run<TBatchPipelineEndpoint>(params object[] constructorParameters)
            where TBatchPipelineEndpoint : IBatchPipelineEndpoint<T>;

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        IBatchPipelineBuilder<T> Use(Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>> middleware);

        /// <summary>
        ///     Adds an <see cref="IMiddleware{T}" /> type to the pipeline
        /// </summary>
        /// <param name="constructorParameters">Parameters to be passed into the middleware constructor</param>
        /// <returns></returns>
        IBatchPipelineBuilder<T> Use<TBatchMiddleware>(params object[] constructorParameters)
            where TBatchMiddleware : IBatchMiddleware<T>;
    }
}