using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenMessage.Providers.Azure.Management
{
    internal sealed class AwaitableLazy<T>
    {
        private readonly Lazy<Task<T>> _instance;

        public AwaitableLazy(Func<Task<T>> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        internal T Value  => _instance.Value.Result;
        internal bool IsValueCreated => _instance.IsValueCreated;
                
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TaskAwaiter<T> GetAwaiter() => _instance.Value.GetAwaiter();
    }
}
