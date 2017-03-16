using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenMessage
{
    internal sealed class BrokerHost : IBrokerHost
    {
        private readonly IList<IBroker> _brokers;
        private bool _disposed;

        public BrokerHost(IEnumerable<IBroker> brokers)
        {
            if (brokers == null)
                throw new ArgumentNullException(nameof(brokers));

            _brokers = brokers.ToList();
        }


        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BrokerHost));

            _disposed = true;

            foreach (var disposable in _brokers)
                disposable.Dispose();

            _brokers.Clear();
        }
    }
}
