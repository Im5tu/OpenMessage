using Microsoft.ServiceBus.Messaging;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.Collections.Generic;

namespace OpenMessage.Providers.Azure.Management
{
    internal abstract class ClientBase<T> : IDisposable
    {
        private readonly List<Action<T>> _callbacks = new List<Action<T>>();
        private readonly ISerializationProvider _provider;

        protected ClientBase(ISerializationProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _provider = provider;
        }

        protected void AddCallback(Action<T> callback)
        {
            // TODO :: argument checking
            _callbacks.Add(callback);
        }

        protected void OnMessage(BrokeredMessage message)
        {
            // TODO :: argument checking
            var entity = Deserialize(message);
            foreach (var callback in _callbacks)
                callback(entity);
        }

        protected BrokeredMessage Serialize(T entity) => _provider.Serialize(entity);

        // TODO :: argument checking
        private T Deserialize(BrokeredMessage message) => _provider.Deserialize<T>(message);
        
        public void Dispose()
        {
            Dispose(true);
        }
        public virtual void Dispose(bool disposing)
        {
        }
    }
}
