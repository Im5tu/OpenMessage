using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Testing.Memory
{
    internal sealed class AwaitableMemoryDispatcher<T> : DispatcherBase<T>
    {
        private readonly ChannelWriter<Message<T>> _channelWriter;

        public AwaitableMemoryDispatcher(ChannelWriter<Message<T>> channelWriter, ILogger<AwaitableMemoryDispatcher<T>> logger)
            : base(logger) => _channelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));

        public override async Task DispatchAsync(Message<T> entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            cancellationToken.ThrowIfCancellationRequested();

            LogDispatch(entity);

            if (!await _channelWriter.WaitToWriteAsync(cancellationToken))
                Throw.Exception("Cannot write to channel");

            var awaitableMessage = new AwaitableMessage<T>(entity);

            await _channelWriter.WriteAsync(awaitableMessage, cancellationToken);

            await awaitableMessage;
        }
    }
}