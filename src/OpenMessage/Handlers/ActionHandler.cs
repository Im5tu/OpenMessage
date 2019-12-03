using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Handlers
{
    internal sealed class ActionHandler<T> : IHandler<T>
    {
        private readonly Func<Message<T>, CancellationToken, Task> _action;

        public ActionHandler(Action<Message<T>> action)
            : this((msg, ct) =>
            {
                action(msg);

                return Task.CompletedTask;
            }) { }

        public ActionHandler(Action<Message<T>, CancellationToken> action)
            : this((msg, ct) =>
            {
                action(msg, ct);

                return Task.CompletedTask;
            }) { }

        public ActionHandler(Func<Message<T>, Task> action)
            : this((msg, ct) => action(msg)) { }

        public ActionHandler(Func<Message<T>, CancellationToken, Task> action) => _action = action;

        public Task HandleAsync(Message<T> message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _action(message, cancellationToken);
        }
    }
}