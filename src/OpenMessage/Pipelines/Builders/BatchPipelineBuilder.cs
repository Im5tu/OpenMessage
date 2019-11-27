using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines.Endpoints;
using OpenMessage.Pipelines.Middleware;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    /// https://github.com/aspnet/HttpAbstractions/blob/master/src/Microsoft.AspNetCore.Http/Internal/ApplicationBuilder.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BatchPipelineBuilder<T> : IBatchPipelineBuilder<T>
    {
        private readonly IList<Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>>> _middleware = new List<Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>>>();
        
        public IBatchPipelineBuilder<T> Use(Func<PipelineDelegate.BatchMiddleware<T>, PipelineDelegate.BatchMiddleware<T>> middleware)
        {
            _middleware.Add(middleware);

            return this;
        }

        public IBatchPipelineBuilder<T> Use<TMiddleware>(params object[] constructorParameters) where TMiddleware : IBatchMiddleware<T>
        {
            _middleware.Add(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    IBatchMiddleware<T> middleware = constructorParameters.Any()
                        ? ActivatorUtilities.CreateInstance<TMiddleware>(messageContext.ServiceProvider, constructorParameters)
                        : messageContext.ServiceProvider.GetRequiredService<TMiddleware>();

                    return middleware.Invoke(messages, cancellationToken, messageContext, next);
                };
            });

            return this;
        }

        public void Run(Func<PipelineDelegate.BatchMiddleware<T>> endpoint)
        {
            _middleware.Add(_ => endpoint());
        }

        public void Run<TBatchPipelineEndpoint>(params object[] constructorParameters) where TBatchPipelineEndpoint : IBatchPipelineEndpoint<T>
        {
            _middleware.Add(_ =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    var pipelineEndpoint = constructorParameters.Any()
                        ? ActivatorUtilities.CreateInstance<TBatchPipelineEndpoint>(messageContext.ServiceProvider, constructorParameters)
                        : messageContext.ServiceProvider.GetRequiredService<TBatchPipelineEndpoint>();

                    return pipelineEndpoint.Invoke(message, cancellationToken, messageContext);
                };
            });
        }

        public PipelineDelegate.SingleMiddleware<T> Build()
        {
            Run<BatchHandlerPipelineEndpoint<T>>();

            PipelineDelegate.BatchMiddleware<T> batchApp = (messages, cancellationToken, context) => Task.CompletedTask;

            foreach (var middleware in _middleware.Reverse())
            {
                batchApp = middleware(batchApp);
            }

            //Create a special piece of endpoint that funnels single message into a batch
            return async (message, cancellationToken, context) =>
            {
                var batcher = context.ServiceProvider.GetRequiredService<ShittyBatcher<Message<T>>>();

                await batcher.Add(message, async messages =>
                {
                    //Is this the right place to do this?
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<BatchPipelineBuilder<T>>>();
                    var serviceScopeFactory = context.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

                    using(var scope = serviceScopeFactory.CreateScope())
                    using (logger.BeginScope($"Batch - {messages.Count} Items"))
                    {
                        await batchApp.Invoke(messages, new CancellationToken(), new MessageContext(scope.ServiceProvider));

                    }
                });
            };
        }
    }
}