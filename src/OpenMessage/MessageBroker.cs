using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace OpenMessage
{
    internal sealed class MessageBroker<T> : IBroker<T>
    {
        private readonly HashSet<IDisposable> _subscriptions = new HashSet<IDisposable>();
        private readonly HashSet<IObservable<T>> _observables = new HashSet<IObservable<T>>();
        private readonly HashSet<IObserver<T>> _observers;
        private readonly ILogger<MessageBroker<T>> _logger;

        public MessageBroker(IEnumerable<IObserver<T>> observers,
            IEnumerable<IObservable<T>> observables,
            ILogger<MessageBroker<T>> logger)
        {
            if (observers == null)
                throw new ArgumentNullException(nameof(observers));

            if (observables == null)
                throw new ArgumentNullException(nameof(observables));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _observers = new HashSet<IObserver<T>>(observers);
            _logger = logger;

            foreach (var observer in _observers)
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

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (_observers)
            {
                _observers.Add(observer);
                return new Disposable(() =>
                {
                    lock (_observers)
                        if(_observers.Remove(observer))
                            observer.OnCompleted();
                });
            }
        }

        private void Notify(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_observers)
            {
                var errors = new List<Exception>();
                foreach (var observer in _observers)
                    try
                    {
                        observer.OnNext(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                        errors.Add(ex);
                    }

                if (errors.Count > 0)
                    throw new AggregateException(errors);
            }
        }

        public void Dispose()
        {
            lock (_observers)
            {
                foreach (var subscription in _subscriptions)
                    subscription.Dispose();

                foreach (var observer in _observers)
                    observer.OnCompleted();

                _observers.Clear();
                _subscriptions.Clear();
            }
        }
    }
}
