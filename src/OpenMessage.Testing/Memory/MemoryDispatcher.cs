using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenMessage.Extensions;

namespace OpenMessage.Testing.Memory
{
    internal sealed class AwaitableMemoryDispatcher<T> : IDispatcher<T>
    {
        private readonly ChannelWriter<Message<T>> _channelWriter;

        public AwaitableMemoryDispatcher(ChannelWriter<Message<T>> channelWriter)
        {
            _channelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
        }

        public async Task DispatchAsync(Message<T> entity, CancellationToken cancellationToken)
        {
            entity.Must(nameof(entity)).NotBeNull();
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _channelWriter.WaitToWriteAsync(cancellationToken))
                Throw.Exception("Cannot write to channel");

            var awaitableMessage = new AwaitableMessage<T>(entity);

            await _channelWriter.WriteAsync(awaitableMessage, cancellationToken);

            await Task.Delay(1000);

            await awaitableMessage;
        }

        public Task DispatchAsync(T entity, CancellationToken cancellationToken)
        {
            return DispatchAsync(new Message<T>{Value = entity }, cancellationToken);
        }
    }
}