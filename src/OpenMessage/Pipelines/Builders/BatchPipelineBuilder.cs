using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Pipelines.Endpoints;
using OpenMessage.Pipelines.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    ///     https://github.com/aspnet/HttpAbstractions/blob/master/src/Microsoft.AspNetCore.Http/Internal/ApplicationBuilder.cs
    /// </summary>
    internal sealed class BatchPipelineBuilder<T> : IBatchPipelineBuilder<T>
    {
        private readonly IMessagingBuilder _builder;
        private readonly IList<Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>>> _middleware = new List<Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>>>();

        /// <inheritdoc />
        public IServiceCollection Services => _builder.Services;

        public BatchPipelineBuilder(IMessagingBuilder builder)
        {
            _builder = builder;
            _builder.Services.AddSingleton<IBatchPipelineBuilder<T>>(this);
        }

        public PipelineDelegate.BatchMiddleware<T> Build()
        {
            Run<BatchHandlerPipelineEndpoint<T>>();

            PipelineDelegate.BatchMiddleware<T> batchApp = (messages, cancellationToken, context) => Task.CompletedTask;

            foreach (var middleware in _middleware.Reverse())
                batchApp = middleware(batchApp);

            return batchApp;
        }

        public void Run(Func<PipelineDelegate.BatchMiddleware<T>> endpoint)
        {
            _middleware.Add(_ => endpoint());
        }

        public void Run<TBatchPipelineEndpoint>(params object[] constructorParameters)
            where TBatchPipelineEndpoint : IBatchPipelineEndpoint<T>
        {
            _middleware.Add(_ =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    var pipelineEndpoint = constructorParameters.Any() ? ActivatorUtilities.CreateInstance<TBatchPipelineEndpoint>(messageContext.ServiceProvider, constructorParameters) : messageContext.ServiceProvider.GetRequiredService<TBatchPipelineEndpoint>();

                    return pipelineEndpoint.Invoke(message, cancellationToken, messageContext);
                };
            });
        }

        public IBatchPipelineBuilder<T> Use(Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>> middleware)
        {
            _middleware.Add(middleware);

            return this;
        }

        public IBatchPipelineBuilder<T> Use<TMiddleware>(params object[] constructorParameters)
            where TMiddleware : IBatchMiddleware<T>
        {
            _middleware.Add(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    IBatchMiddleware<T> middleware = constructorParameters.Any() ? ActivatorUtilities.CreateInstance<TMiddleware>(messageContext.ServiceProvider, constructorParameters) : messageContext.ServiceProvider.GetRequiredService<TMiddleware>();

                    return middleware.Invoke(messages, cancellationToken, messageContext, next);
                };
            });

            return this;
        }
    }
}