using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ClientBase<T>> _logger;

        protected ClientBase(ISerializationProvider provider,
            ILogger<ClientBase<T>> logger)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _provider = provider;
            _logger = logger;
        }

        protected void AddCallback(Action<T> callback)
        {
            // TODO :: argument checking
            _callbacks.Add(callback);
        }

        protected void OnMessage(BrokeredMessage message)
        {
            try
            {
                // TODO :: argument checking
                var entity = Deserialize(message);
                foreach (var callback in _callbacks)
                    callback(entity);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                message.Abandon(new Dictionary<string, object>
                {
                    { "Exception", ex.Message }
                });
                throw;
            }
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
