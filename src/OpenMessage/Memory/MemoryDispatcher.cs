using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenMessage.Extensions;

namespace OpenMessage.Memory
{
    internal sealed class MemoryDispatcher<T> : IDispatcher<T>
    {
        private readonly ChannelWriter<Message<T>> _channelWriter;
        private readonly IOptions<MemoryOptions<T>> _options;

        public MemoryDispatcher(ChannelWriter<Message<T>> channelWriter, IOptions<MemoryOptions<T>> options)
        {
            _channelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
            _options = options;
        }

        public Task DispatchAsync(Message<T> entity, CancellationToken cancellationToken)
        {
            if (!_options.Value.DispatcherFireAndForget)
            {
                entity = new AwaitableMessage<T>(entity);
            }

            return Dispatch(entity, cancellationToken);
        }

        public Task DispatchAsync(T entity, CancellationToken cancellationToken)
        {
            var message = _options.Value.DispatcherFireAndForget
                ? new Message<T> { Value = entity }
                : new AwaitableMessage<T> { Value = entity };

            return DispatchAsync(message, cancellationToken);
        }

        private async Task Dispatch(Message<T> entity, CancellationToken cancellationToken)
        {
            entity.Must(nameof(entity)).NotBeNull();
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _channelWriter.WaitToWriteAsync(cancellationToken))
                Throw.Exception("Cannot write to channel");

            await _channelWriter.WriteAsync(entity, cancellationToken);

            if(entity is AwaitableMessage<T> awaitableMessage)
            {
                await awaitableMessage;
            }
        }
    }
}