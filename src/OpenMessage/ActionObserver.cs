using System;

namespace OpenMessage
{
    internal sealed class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<T> _action;
        private bool _completed = false;

        internal ActionObserver(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _action = action;
        }

        public void OnCompleted()
        {
            _completed = true;
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            if (_completed)
                return;

            _action(value);
        }
    }
}
