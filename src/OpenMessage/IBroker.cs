using System;

namespace OpenMessage
{
    /// <summary>
    ///     Marker interface for IBroker<T> making DI scenarios easier.
    /// </summary>
    public interface IBroker : IDisposable
    {
    }

    /// <summary>
    ///     Receives messages before handing off to observers.
    /// </summary>
    public interface IBroker<T> : IObservable<T>, IBroker
    {
    }
}
