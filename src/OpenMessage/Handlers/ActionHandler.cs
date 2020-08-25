using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Handlers
{
    internal sealed class ActionHandler<T> : Handler<T>
    {
        private readonly Func<Message<T>, CancellationToken, Task> _action;

        public ActionHandler(Action<Message<T>> action, ILogger<ActionHandler<T>> logger)
            : this((msg, ct) => Task.Run(() => action(msg), ct), logger) { }

        public ActionHandler(Action<Message<T>, CancellationToken> action, ILogger<ActionHandler<T>> logger)
            : this((msg, ct) => Task.Run(() => action(msg, ct), ct), logger) { }

        public ActionHandler(Func<Message<T>, Task> action, ILogger<ActionHandler<T>> logger)
            : this((msg, ct) => action(msg), logger) { }

        public ActionHandler(Func<Message<T>, CancellationToken, Task> action, ILogger<ActionHandler<T>> logger) : base(logger) => _action = action;

        protected override Task OnHandleAsync(Message<T> message, CancellationToken cancellationToken)
        {
            return _action(message, cancellationToken);
        }
    }
}