using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Memory
{
    internal sealed class MemoryDispatcher<T> : DispatcherBase<T>
    {
        private readonly ChannelWriter<Message<T>> _channelWriter;

        public MemoryDispatcher(ChannelWriter<Message<T>> channelWriter, ILogger<MemoryDispatcher<T>> logger)
            : base(logger)
        {
            _channelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
        }

        public override async Task DispatchAsync(Message<T> entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                Throw.ArgumentNullException(nameof(entity));

            cancellationToken.ThrowIfCancellationRequested();

            LogDispatch(entity);

            if (!await _channelWriter.WaitToWriteAsync(cancellationToken))
                Throw.Exception("Cannot write to channel");

            await _channelWriter.WriteAsync(entity, cancellationToken);
        }
    }
}