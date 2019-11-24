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
}