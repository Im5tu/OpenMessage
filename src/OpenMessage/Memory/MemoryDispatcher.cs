using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenMessage.Extensions;

namespace OpenMessage.Memory
{
    internal sealed class MemoryDispatcher<T> : IDispatcher<T>
    {
        private readonly ChannelWriter<Message<T>> _channelWriter;

        public MemoryDispatcher(ChannelWriter<Message<T>> channelWriter)
        {
            _channelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
        }

        public async Task DispatchAsync(Message<T> entity, CancellationToken cancellationToken)
        {
            entity.Must(nameof(entity)).NotBeNull();
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _channelWriter.WaitToWriteAsync(cancellationToken))
                Throw.Exception("Cannot write to channel");

            await _channelWriter.WriteAsync(entity, cancellationToken);
        }

        public Task DispatchAsync(T entity, CancellationToken cancellationToken)
        {
            return DispatchAsync(new Message<T> {Value = entity}, cancellationToken);
        }
    }
}