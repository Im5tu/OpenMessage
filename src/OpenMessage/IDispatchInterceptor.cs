namespace OpenMessage
{
    /// <summary>
    ///     Allows messages to be intercepted prior to sending/send modification.
    /// </summary>
    public interface IDispatchInterceptor<T>
    {
        /// <summary>
        ///     Determines whether or not the entity should be stopped from sending. A return value of true signals the message should be stopped from sending.
        /// </summary>
        bool Intercept(T entity);
    }
}
