using System;
using System.Threading;
using System.Threading.Tasks;
using OpenMessage.Pipelines.Endpoints;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    /// Helpers for configuring a <see cref="PipelineBuilder{T}"/>
    /// </summary>
    public static class PipelineBuilderExtensions
    {
        #region Use

        /// <summary>
        /// Adds a middleware step into the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> builder, Func<Message<T>, CancellationToken, MessageContext, Func<Message<T>, CancellationToken, MessageContext, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, cancellationToken, messageContext, (m, ct, ctx) => next(m, ct, ctx));
                };
            });
        }

        /// <summary>
        /// Adds a middleware step into the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> builder, Func<Message<T>, CancellationToken, MessageContext, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, cancellationToken, messageContext, () => next(message, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        /// Adds a middleware step into the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> builder, Func<Message<T>, CancellationToken, Func<Message<T>, CancellationToken, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, cancellationToken, (m, ctx) => next(m, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        /// Adds a middleware step into the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> builder, Func<Message<T>, CancellationToken, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, cancellationToken, () => next(message, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        /// Adds a middleware step into the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> builder, Func<Message<T>, Func<Message<T>, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, m => next(m, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        /// Adds a middleware step into the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> builder, Func<Message<T>, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, () => next(message, cancellationToken, messageContext));
                };
            });
        }

        #endregion

        #region Run

        /// <summary>
        /// Ends the pipeline by executing the provided endpoint. Defaults to <see cref="HandlerPipelineEndpoint{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="action"></param>
        public static void Run<T>(this IPipelineBuilder<T> builder, Func<Message<T>, CancellationToken, MessageContext, Task> action)
        {
            builder.Run(() =>
            {
                return (message, cancellationToken, messageContext) => action(message, cancellationToken, messageContext);
            });
        }

        /// <summary>
        /// Ends the pipeline by executing the provided endpoint. Defaults to <see cref="HandlerPipelineEndpoint{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="action"></param>
        public static void Run<T>(this IPipelineBuilder<T> builder, Func<Message<T>, CancellationToken, Task> action)
        {
            builder.Run(() =>
            {
                return (message, cancellationToken, messageContext) => action(message, cancellationToken);
            });
        }

        /// <summary>
        /// Ends the pipeline by executing the provided endpoint. Defaults to <see cref="HandlerPipelineEndpoint{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="action"></param>
        public static void Run<T>(this IPipelineBuilder<T> builder, Func<Message<T>, Task> action)
        {
            builder.Run(() =>
            {
                return (message, cancellationToken, messageContext) => action(message);
            });
        }

        #endregion
    }
}