using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.Handlers;

namespace OpenMessage.Pipelines
{
    /// <summary>
    ///     A simplistic implementation of <see cref="IPipeline{T}" />
    /// </summary>
    /// <typeparam name="T">The type the pipeline consumes</typeparam>
    public class SimplePipeline<T> : IPipeline<T>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        ///     The logger to use
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        ///     The current options for the pipeline
        /// </summary>
        protected IOptionsMonitor<PipelineOptions<T>> Options { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory to use</param>
        /// <param name="options">The options to use</param>
        /// <param name="logger">The logger</param>
        public SimplePipeline(IServiceScopeFactory serviceScopeFactory,
            IOptionsMonitor<PipelineOptions<T>> options,
            ILogger<SimplePipeline<T>> logger)
            : this(serviceScopeFactory, options, (ILogger) logger)
        {
        }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory to use</param>
        /// <param name="options">The options to use</param>
        /// <param name="logger">The logger</param>
        protected SimplePipeline(IServiceScopeFactory serviceScopeFactory,
            IOptionsMonitor<PipelineOptions<T>> options,
            ILogger logger)
        {
            _services = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritDoc />
        public async Task HandleAsync(Batch<T> message, CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();

            await OnHandleAsync(scope.ServiceProvider, message, cancellationToken);
        }

        /// <summary>
        ///     Handles the specified batch
        /// </summary>
        /// <param name="serviceScope">The scoped service provider</param>
        /// <param name="batch">The batch to handle</param>
        /// <param name="cancellationToken">The cancellation token to use for asynchronous operations</param>
        /// <returns>A task that completes when the batch has been handled</returns>
        protected virtual async Task OnHandleAsync(IServiceProvider serviceScope, Batch<T> batch, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var handler in serviceScope.GetRequiredService<IEnumerable<IHandler<T>>>())
                {
                    foreach (var message in batch)
                    {
                        await handler.HandleAsync(message, cancellationToken);
                    }
                }

                foreach (var handler in serviceScope.GetRequiredService<IEnumerable<IBatchHandler<T>>>())
                {
                    await handler.HandleAsync(batch.Messages, cancellationToken);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}