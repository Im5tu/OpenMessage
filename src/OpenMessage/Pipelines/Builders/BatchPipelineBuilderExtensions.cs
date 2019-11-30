using OpenMessage.Pipelines.Endpoints;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    ///     Helpers for configuring a <see cref="BatchPipelineBuilder{T}" />
    /// </summary>
    public static class BatchPipelineBuilderExtensions
    {
        #region Use

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        public static IBatchPipelineBuilder<T> Use<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, CancellationToken, MessageContext, Func<IReadOnlyCollection<Message<T>>, CancellationToken, MessageContext, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    return middleware(messages, cancellationToken, messageContext, (m, ct, ctx) => next(m, ct, ctx));
                };
            });
        }

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        public static IBatchPipelineBuilder<T> Use<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, CancellationToken, MessageContext, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    return middleware(messages, cancellationToken, messageContext, () => next(messages, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        public static IBatchPipelineBuilder<T> Use<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, CancellationToken, Func<IReadOnlyCollection<Message<T>>, CancellationToken, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    return middleware(messages, cancellationToken, (m, ctx) => next(m, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        public static IBatchPipelineBuilder<T> Use<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, CancellationToken, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    return middleware(messages, cancellationToken, () => next(messages, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        public static IBatchPipelineBuilder<T> Use<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, Func<IReadOnlyCollection<Message<T>>, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    return middleware(messages, m => next(m, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        ///     Adds a middleware step into the pipeline
        /// </summary>
        public static IBatchPipelineBuilder<T> Use<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    return middleware(messages, () => next(messages, cancellationToken, messageContext));
                };
            });
        }

        #endregion

        #region Run

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        public static void Run<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, CancellationToken, MessageContext, Task> action)
        {
            builder.Run(() =>
            {
                return (messages, cancellationToken, messageContext) => action(messages, cancellationToken, messageContext);
            });
        }

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        public static void Run<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, CancellationToken, Task> action)
        {
            builder.Run(() =>
            {
                return (messages, cancellationToken, messageContext) => action(messages, cancellationToken);
            });
        }

        /// <summary>
        ///     Ends the pipeline by executing the provided endpoint. Defaults to <see cref="BatchHandlerPipelineEndpoint{T}" />
        /// </summary>
        public static void Run<T>(this IBatchPipelineBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, Task> action)
        {
            builder.Run(() =>
            {
                return (messages, cancellationToken, messageContext) => action(messages);
            });
        }

        #endregion
    }
}