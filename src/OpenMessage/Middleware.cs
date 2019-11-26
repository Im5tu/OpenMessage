using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenMessage
{
    public static class BatchMiddlewareBuilderExtensions
    {
        /// <summary>
        /// simplest
        /// </summary>
        public static IBatchMiddlewareBuilder<T> Use<T>(this IBatchMiddlewareBuilder<T> builder, Func<IReadOnlyCollection<Message<T>>, Func<Task>, Task> middleware)
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

    /// <summary>
    /// https://github.com/aspnet/HttpAbstractions/blob/master/src/Microsoft.AspNetCore.Http/Internal/ApplicationBuilder.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MiddlewareBuilder<T> : IMiddlewareBuilder<T>
    {
        private readonly IList<Func<MessageDelegate.Single<T>, MessageDelegate.Single<T>>> _middlewares = new List<Func<MessageDelegate.Single<T>, MessageDelegate.Single<T>>>();

        public IMiddlewareBuilder<T> Use(Func<MessageDelegate.Single<T>, MessageDelegate.Single<T>> middleware)
        {
            _middlewares.Add(middleware);

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
            return new BatchMiddlewareBuilder<T>(this);
        }

        public MessageDelegate.Single<T> Build(MessageDelegate.Single<T> app = null)
        {
            if (app == null)
            {
                app = (message, cancellationToken, context) =>
                {
                    context.ServiceProvider.GetRequiredService<ILogger<MessageDelegate.Single<T>>>().LogInformation($"Call the handlers {message}");

                    return Task.CompletedTask;
                };
            }

            foreach (var middleware in _middlewares.Reverse())
            {
                app = middleware(app);
            }

            return app;
        }
    }

    /// <summary>
    /// https://github.com/aspnet/HttpAbstractions/blob/master/src/Microsoft.AspNetCore.Http/Internal/ApplicationBuilder.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BatchMiddlewareBuilder<T> : IBatchMiddlewareBuilder<T>
    {
        private readonly IList<Func<MessageDelegate.Batch<T>, MessageDelegate.Batch<T>>> _middlewares = new List<Func<MessageDelegate.Batch<T>, MessageDelegate.Batch<T>>>();
        private readonly MiddlewareBuilder<T> _middlewareBuilder;

        public BatchMiddlewareBuilder(MiddlewareBuilder<T> middlewareBuilder)
        {
            _middlewareBuilder = middlewareBuilder;
        }

        public IBatchMiddlewareBuilder<T> Use(Func<MessageDelegate.Batch<T>, MessageDelegate.Batch<T>> middleware)
        {
            _middlewares.Add(middleware);

            return this;
        }

        public IBatchMiddlewareBuilder<T> Use<TMiddleware>(params object[] constructorParameters) where TMiddleware : IBatchMiddleware<T>
        {
            return Use(next =>
            {
                return (messages, cancellationToken, messageContext) =>
                {
                    IBatchMiddleware<T> middleware = constructorParameters.Any()
                        ? ActivatorUtilities.CreateInstance<TMiddleware>(messageContext.ServiceProvider, constructorParameters)
                        : messageContext.ServiceProvider.GetRequiredService<TMiddleware>();

                    return middleware.Invoke(messages, cancellationToken, messageContext, next);
                };
            });
        }

        public MessageDelegate.Single<T> Build()
        {
            MessageDelegate.Batch<T> batchApp = (messages, cancellationToken, context) =>
            {
                context.ServiceProvider.GetRequiredService<ILogger<MessageDelegate.Single<T>>>().LogInformation($"Call the batch handlers {messages}");

                return Task.CompletedTask;
            };

            foreach (var middleware in _middlewares.Reverse())
            {
                batchApp = middleware(batchApp);
            }

            return _middlewareBuilder.Build((message, token, context) =>
            {
                //create a new scope
                //create a new logging context
                //combine messages
                return batchApp(new[] {message}, token, context);
            });
        }
    }

    public class MessageDelegate
    {
        public delegate Task Single<T>(Message<T> message, CancellationToken cancellationToken, MessageContext context);
        public delegate Task Batch<T>(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext context);
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
        IMiddlewareBuilder<T> Use(Func<MessageDelegate.Single<T>, MessageDelegate.Single<T>> middleware);
        IMiddlewareBuilder<T> Use<TMiddleware>(params object[] constructorParameters) where TMiddleware : IMiddleware<T>;
        IBatchMiddlewareBuilder<T> Batch();
        MessageDelegate.Single<T> Build(MessageDelegate.Single<T> app = null);
    }

    public interface IBatchMiddlewareBuilder<T>
    {
        IBatchMiddlewareBuilder<T> Use(Func<MessageDelegate.Batch<T>, MessageDelegate.Batch<T>> middleware);
        IBatchMiddlewareBuilder<T> Use<TBatchMiddleware>(params object[] constructorParameters) where TBatchMiddleware : IBatchMiddleware<T>;
        MessageDelegate.Single<T> Build();
    }

    public interface IMiddleware<T>
    {
        Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, MessageDelegate.Single<T> next);
    }

    public interface IBatchMiddleware<T>
    {
        Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext, MessageDelegate.Batch<T> next);
    }
}
