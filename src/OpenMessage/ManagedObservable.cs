using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace OpenMessage
{
    internal abstract class ManagedObservable<T> : IObservable<T>, IDisposable
    {
        private readonly ILogger<ManagedObservable<T>> _logger;
        private readonly HashSet<IObserver<T>> _observers = new HashSet<IObserver<T>>();

        public ManagedObservable(ILogger<ManagedObservable<T>> logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
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
                        if (_observers.Remove(observer))
                            observer.OnCompleted();
                });
            }
        }

        protected void Notify(T entity)
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
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                    observer.OnCompleted();

                _observers.Clear();
            }
        }
    }
}
