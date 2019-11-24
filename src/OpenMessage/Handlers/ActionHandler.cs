using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Handlers
{
    internal sealed class ActionBatchHandler<T> : IBatchHandler<T>
    {
        private readonly Func<IReadOnlyCollection<Message<T>>, CancellationToken, Task> _action;

        public ActionBatchHandler(Func<IReadOnlyCollection<Message<T>>, CancellationToken, Task> action)
        {
            _action = action;
        }

        public ActionBatchHandler(Func<IReadOnlyCollection<Message<T>>, Task> action)
            : this((batch, cancellationToken) => action(batch))
        {
        }

        public ActionBatchHandler(Action<IReadOnlyCollection<Message<T>>> action)
            : this((batch, cancellationToken) =>
            {
                action(batch);
                return Task.CompletedTask;
            })
        {
        }

        public Task HandleAsync(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken) => _action(messages, cancellationToken);
    }

    internal sealed class ActionHandler<T> : IHandler<T>
    {
        private readonly Func<Message<T>, CancellationToken, Task> _action;

        public ActionHandler(Action<Message<T>> action)
            : this((msg, ct) =>
            {
                action(msg);
                return Task.CompletedTask;
            })
        {
        }

        public ActionHandler(Action<Message<T>, CancellationToken> action)
            : this((msg, ct) =>
            {
                action(msg, ct);
                return Task.CompletedTask;
            })
        {
        }

        public ActionHandler(Func<Message<T>, Task> action)
            : this((msg, ct) => action(msg))
        {
        }

        public ActionHandler(Func<Message<T>, CancellationToken, Task> action)
        {
            _action = action;
        }

        public Task HandleAsync(Message<T> message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _action(message, cancellationToken);
        }
    }
}