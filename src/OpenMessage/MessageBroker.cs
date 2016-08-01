using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenMessage
{
    internal sealed class MessageBroker<T> : ManagedObservable<T>, IBroker<T>
    {
        private readonly HashSet<IDisposable> _subscriptions = new HashSet<IDisposable>();
        private readonly HashSet<IObservable<T>> _observables = new HashSet<IObservable<T>>();

        public MessageBroker(IEnumerable<IObserver<T>> observers,
            IEnumerable<IObservable<T>> observables,
            ILogger<ManagedObservable<T>> logger)
            : base(logger)
        {
            if (observers == null)
                throw new ArgumentNullException(nameof(observers));

            if (observables == null)
                throw new ArgumentNullException(nameof(observables));

            foreach (var observer in observers)
                _subscriptions.Add(Subscribe(observer));

            foreach (var observable in observables)
            {
                _observables.Add(observable);

                var disposableObservable = observable as IDisposable;
                if (disposableObservable != null)
                    _subscriptions.Add(disposableObservable);

                _subscriptions.Add(observable.Subscribe(new ActionObserver<T>(Notify)));
            }
        }

        public override void Dispose(bool disposing)
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            foreach (var observable in _observables.OfType<IDisposable>())
                observable.Dispose();
        }
    }
}
