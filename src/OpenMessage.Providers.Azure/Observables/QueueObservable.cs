using Microsoft.Extensions.Logging;
using OpenMessage.Providers.Azure.Management;
using System;

namespace OpenMessage.Providers.Azure.Observables
{
    internal sealed class QueueObservable<T> : ManagedObservable<T>
    {
        private readonly IQueueClient<T> _queueClient;

        public QueueObservable(ILogger<ManagedObservable<T>> logger,
            IQueueFactory<T> queueFactory) : base(logger)
        {
            if (queueFactory == null)
                throw new ArgumentNullException(nameof(queueFactory));

            _queueClient = queueFactory.Create();
            _queueClient.RegisterCallback(Notify);
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _queueClient?.Dispose();
        }
    }
}
