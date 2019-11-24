using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines;

namespace OpenMessage.Memory
{
    internal sealed class MemoryMessagePump<T> : MessagePump<T>
    {
        private readonly ChannelWriter<Message<T>> _channelWriter;
        private readonly string _consumerId;

        public MemoryMessagePump(ChannelWriter<Message<T>> channelWriter, ILogger<MemoryMessagePump<T>> logger, string consumerId)
            : base(channelWriter, logger)
        {
            _channelWriter = channelWriter;
            _consumerId = consumerId;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (MemoryDispatcher<T>.Queue.TryDequeue(out var message))
                {
                    await _channelWriter.WriteAsync(message, cancellationToken);
                }
            }
        }
    }
}