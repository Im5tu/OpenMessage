using System;
using System.Threading.Tasks;

namespace OpenMessage
{
    public static class DispatcherExtensions
    {
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));

            return dispatcher.DispatchAsync(entity, TimeSpan.MinValue);
        }
    }
}
