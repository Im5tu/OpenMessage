using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsDispatcherService : BackgroundService
    {
        private readonly ChannelReader<SendSqsMessageCommand> _messageReader;
        private readonly ILogger<SqsDispatcherService> _logger;
        private readonly Dictionary<string, AmazonSQSClient> _clients = new Dictionary<string, AmazonSQSClient>(StringComparer.Ordinal);

        private readonly Dictionary<string, Channel<SendSqsMessageCommand>> _channels = new Dictionary<string, Channel<SendSqsMessageCommand>>(StringComparer.Ordinal);
        private readonly Dictionary<string, Task> _channelReaderTasks = new Dictionary<string, Task>();

        public SqsDispatcherService(ChannelReader<SendSqsMessageCommand> messageReader, ILogger<SqsDispatcherService> logger)
        {
            _messageReader = messageReader;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
            await Task.Yield();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_messageReader.TryRead(out var msg))
                    {
                        if (msg.QueueUrl is null)
                        {
                            msg.Exception(new Exception("Cannot process message without a destination queue url"));
                            continue;
                        }

                        if (msg.Message is null)
                        {
                            msg.Exception(new Exception("Cannot process message without a message to send"));
                            continue;
                        }

                        if (!_channels.TryGetValue(msg.LookupKey, out var channel))
                        {
                            _channels[msg.LookupKey] = channel = Channel.CreateUnbounded<SendSqsMessageCommand>(new UnboundedChannelOptions
                            {
                                SingleReader = true,
                                SingleWriter = true
                            });
                            _channelReaderTasks[msg.LookupKey] = Task.Run(async () =>
                            {
                                var messagesToSend = new List<SendSqsMessageCommand>(10);
                                while (!cancellationToken.IsCancellationRequested)
                                {
                                    try
                                    {
                                        if (messagesToSend is null)
                                            continue;

                                        var readMessage = channel.Reader.TryRead(out var msg);
                                        if (readMessage)
                                            messagesToSend.Add(msg);

                                        if (messagesToSend.Count == 10 || messagesToSend.Count > 0 && !readMessage)
                                        {
                                            var messages = Interlocked.Exchange(ref messagesToSend, new List<SendSqsMessageCommand>(10));
                                            if (messages is {})
                                                _ = ProcessMessages(messages);
                                        }
                                        else if (!cancellationToken.IsCancellationRequested && !readMessage)
                                            await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
                                    }
                                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                                    {
                                        if (messagesToSend is {})
                                            foreach (var msg in messagesToSend)
                                                msg.Cancel(cancellationToken);
                                    }
                                    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                                    {
                                        if (messagesToSend is {})
                                            foreach (var msg in messagesToSend)
                                                msg.Exception(ex);
                                    }
                                }
                            });
                        }

                        await channel.Writer.WriteAsync(msg, cancellationToken).ConfigureAwait(false);
                    }
                    else
                        await _messageReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

        private async Task ProcessMessages(List<SendSqsMessageCommand> messages)
        {
            if (messages.Count == 0)
                return;

            var firstMessage = messages[0];

            try
            {
                var entries = new List<SendMessageBatchRequestEntry>(messages.Count);
                foreach(var msg in messages)
                    if (msg.Message is {})
                        entries.Add(msg.Message);

                var request = new SendMessageBatchRequest(firstMessage.QueueUrl, entries);
                if (!_clients.TryGetValue(firstMessage.LookupKey, out var client))
                {
                    var config = new AmazonSQSConfig
                    {
                        ServiceURL = firstMessage.ServiceUrl
                    };

                    if (firstMessage.RegionEndpoint != null)
                        config.RegionEndpoint = RegionEndpoint.GetBySystemName(firstMessage.RegionEndpoint);

                    _clients[firstMessage.LookupKey] = client = new AmazonSQSClient(config);
                }

                var response = await client.SendMessageBatchAsync(request);

                // TODO :: we should be able to complete certain messages here
                if (response.Failed.Count > 0)
                    Throw.Exception("One or more messages failed to send");

                foreach (var msg in messages)
                    msg.Complete();
            }
            catch (Exception e)
            {
                foreach (var msg in messages)
                    msg.Exception(e);
            }
        }
    }
}