using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage
{
    public static class MiddlewareBuilderExtensions
    {
        /// <summary>
        /// full fat
        /// </summary>
        public static IMiddlewareBuilder<T> Use<T>(this IMiddlewareBuilder<T> builder, Func<Message<T>, CancellationToken, MessageContext, Func<Message<T>, CancellationToken, MessageContext, Task>, Task> middleware)
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
        /// slimmer
        /// </summary>
        public static IMiddlewareBuilder<T> Use<T>(this IMiddlewareBuilder<T> builder, Func<Message<T>, MessageContext, Func<Message<T>, MessageContext, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, messageContext, (m, ctx) => next(m, cancellationToken, ctx));
                };
            });
        }

        /// <summary>
        /// simple
        /// </summary>
        public static IMiddlewareBuilder<T> Use<T>(this IMiddlewareBuilder<T> builder, Func<Message<T>, Func<Message<T>, Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, (m) => next(m, cancellationToken, messageContext));
                };
            });
        }

        /// <summary>
        /// simplest
        /// </summary>
        public static IMiddlewareBuilder<T> Use<T>(this IMiddlewareBuilder<T> builder, Func<Message<T>, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    return middleware(message, () => next(message, cancellationToken, messageContext));
                };
            });
        }
    }

    internal class MiddlewareBuilder<T> : IMiddlewareBuilder<T>
    {
        public MiddlewareBuilder(IServiceProvider serviceProvider)
        {

        }

        private readonly IList<Func<MessageDelegate.Delegate<T>, MessageDelegate.Delegate<T>>> _pipelines = new List<Func<MessageDelegate.Delegate<T>, MessageDelegate.Delegate<T>>>();

        public IMiddlewareBuilder<T> Use(Func<MessageDelegate.Delegate<T>, MessageDelegate.Delegate<T>> middleware)
        {
            _pipelines.Add(middleware);

            return this;
        }

        public IMiddlewareBuilder<T> Use<TMiddleware>(params object[] constructorParameters) where TMiddleware : IMiddleware<T>
        {
            return Use(next =>
            {
                return (message, cancellationToken, messageContext) =>
                {
                    var middleware = constructorParameters.Any()
                        ? ActivatorUtilities.CreateInstance<TMiddleware>(messageContext.ServiceProvider, constructorParameters)
                        : messageContext.ServiceProvider.GetRequiredService<TMiddleware>();

                    return middleware.Invoke(message, cancellationToken, messageContext, next);
                };
            });
        }

        public IBatchMiddlewareBuilder<T> Batch()
        {
            throw new NotImplementedException();
        }
    }

    public class MessageDelegate
    {
        public delegate Task Delegate<T>(Message<T> message, CancellationToken cancellationToken, MessageContext context);
    }

    public class MessageContext
    {
        public MessageContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }

    public interface IMiddlewareBuilder<T>
    {
        IMiddlewareBuilder<T> Use(Func<MessageDelegate.Delegate<T>, MessageDelegate.Delegate<T>> middleware);
        IBatchMiddlewareBuilder<T> Batch();
    }

    public interface IBatchMiddlewareBuilder<T>
    {
        IBatchMiddleware<T> Use<TBatchPipeline>() where TBatchPipeline : IBatchMiddleware<T>;
        IBatchMiddleware<T> Use<TBatchPipeline>(Func<IReadOnlyCollection<Message<T>>, CancellationToken, Func<IReadOnlyCollection<Message<T>>, CancellationToken, Task>, Task> use);
    }

    public interface IMiddleware<T>
    {
        Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, MessageDelegate.Delegate<T> next);
    }

    public interface IBatchMiddleware<T>
    {
        Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext, MessageDelegate.Delegate<T> next);
    }
}
