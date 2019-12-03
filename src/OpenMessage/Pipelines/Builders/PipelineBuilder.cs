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
    internal sealed class PipelineBuilder<T> : IPipelineBuilder<T>
    {
        private readonly IMessagingBuilder _builder;
        private readonly IList<Func<PipelineDelegate.SingleMiddleware<T>, PipelineDelegate.SingleMiddleware<T>>> _middleware = new List<Func<PipelineDelegate.SingleMiddleware<T>, PipelineDelegate.SingleMiddleware<T>>>();

        public IServiceCollection Services => _builder.Services;

        public PipelineBuilder(IMessagingBuilder builder)
        {
            _builder = builder;
            _builder.Services.AddSingleton<IPipelineBuilder<T>>(this);
        }

        public IBatchPipelineBuilder<T> Batch()
        {
            if (_builder is null)
                throw new Exception($"Batched pipelines can only be created when configured via an {nameof(IMessagingBuilder)}");

            Run<BatchPipelineEndpoint<T>>();

            return new BatchPipelineBuilder<T>(_builder);
        }

        public PipelineDelegate.SingleMiddleware<T> Build()
        {
            Run<HandlerPipelineEndpoint<T>>();

            PipelineDelegate.SingleMiddleware<T> app = (message, cancellationToken, context) => Task.CompletedTask;

            foreach (var middleware in _middleware.Reverse())
                app = middleware(app);

            return app;
        }

        public void Run(Func<PipelineDelegate.SingleMiddleware<T>> endpoint)
        {
            _middleware.Add(_ => endpoint());
        }

        public void Run<TPipelineEndpoint>(params object[] constructorParameters)
            where TPipelineEndpoint : IPipelineEndpoint<T>
        {
            _middleware.Add(_ =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    var pipelineEndpoint = constructorParameters.Any() ? ActivatorUtilities.CreateInstance<TPipelineEndpoint>(messageContext.ServiceProvider, constructorParameters) : messageContext.ServiceProvider.GetRequiredService<TPipelineEndpoint>();

                    return pipelineEndpoint.Invoke(message, cancellationToken, messageContext);
                };
            });
        }

        public IPipelineBuilder<T> Use(Func<PipelineDelegate.SingleMiddleware<T>, PipelineDelegate.SingleMiddleware<T>> middleware)
        {
            _middleware.Add(middleware);

            return this;
        }

        public IPipelineBuilder<T> Use<TMiddleware>(params object[] constructorParameters)
            where TMiddleware : IMiddleware<T>
        {
            _middleware.Add(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    var middleware = constructorParameters.Any() ? ActivatorUtilities.CreateInstance<TMiddleware>(messageContext.ServiceProvider, constructorParameters) : messageContext.ServiceProvider.GetRequiredService<TMiddleware>();

                    return middleware.Invoke(message, cancellationToken, messageContext, next);
                };
            });

            return this;
        }
    }
}