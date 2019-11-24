using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OpenMessage.Extensions;

namespace OpenMessage.Memory
{
    internal sealed class MemoryDispatcher<T> : IDispatcher<T>
    {
        internal static ConcurrentQueue<Message<T>> Queue  = new ConcurrentQueue<Message<T>>();

        public Task DispatchAsync(Message<T> entity, CancellationToken cancellationToken)
        {
            entity.Must(nameof(entity)).NotBeNull();
            cancellationToken.ThrowIfCancellationRequested();

            Queue.Enqueue(entity);

            return Task.CompletedTask;
        }

        public Task DispatchAsync(T entity, CancellationToken cancellationToken)
        {
            return DispatchAsync(new Message<T> {Value = entity}, cancellationToken);
        }
    }
}