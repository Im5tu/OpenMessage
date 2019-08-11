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
        protected IOptionsSnapshot<PipelineOptions<T>> Options { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory to use</param>
        /// <param name="options">The options to use</param>
        /// <param name="logger">The logger</param>
        public SimplePipeline(IServiceScopeFactory serviceScopeFactory,
            IOptionsSnapshot<PipelineOptions<T>> options,
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
            IOptionsSnapshot<PipelineOptions<T>> options,
            ILogger logger)
        {
            _services = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritDoc />
        public async Task HandleAsync(Message<T> message, CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();

            await OnHandleAsync(scope.ServiceProvider, message, cancellationToken);
        }

        /// <summary>
        ///     Handles the specified message
        /// </summary>
        /// <param name="serviceScope">The scoped service provider</param>
        /// <param name="message">The message to handle</param>
        /// <param name="cancellationToken">The cancellation token to use for asynchronous operations</param>
        /// <returns>A task that completes when the message has been handled</returns>
        protected virtual async Task OnHandleAsync(IServiceProvider serviceScope, Message<T> message, CancellationToken cancellationToken)
        {
            foreach (var handler in serviceScope.GetRequiredService<IEnumerable<IHandler<T>>>())
                await handler.HandleAsync(message, cancellationToken);
        }
    }
}