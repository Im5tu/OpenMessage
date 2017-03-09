namespace OpenMessage
{
    public interface IDispatchInterceptor<T>
    {
        bool Intercept(T entity);
    }
}
