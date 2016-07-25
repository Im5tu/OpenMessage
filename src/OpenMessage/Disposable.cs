using System;

namespace OpenMessage
{
    internal sealed class Disposable : IDisposable
    {
        private readonly Action _onDispose;

        internal Disposable(Action onDispose)
        {
            if (onDispose == null)
                throw new ArgumentNullException(nameof(onDispose));

            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
