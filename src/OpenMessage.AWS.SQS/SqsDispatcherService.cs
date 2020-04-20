using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsDispatcherService : BackgroundService
    {
        private readonly ChannelReader<SendSqsMessageCommand> _messageReader;
        private readonly IOptionsMonitor<SQSDispatcherOptions> _optionsMonitor;
        private readonly ILogger<SqsDispatcherService> _logger;

        public SqsDispatcherService(ChannelReader<SendSqsMessageCommand> messageReader, IOptionsMonitor<SQSDispatcherOptions> optionsMonitor, ILogger<SqsDispatcherService> logger)
        {
            _messageReader = messageReader ?? throw new ArgumentNullException(nameof(messageReader));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
            await Task.Yield();

            var queues = new Dictionary<string, List<SendSqsMessageCommand>>(StringComparer.Ordinal);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_messageReader.TryRead(out var msg))
                    {
                        if (msg.QueueUrl is null)
                        {
                            msg.TaskCompletionSource.TrySetException(new Exception("Cannot process message without a destination queue url"));
                            continue;
                        }

                        if (msg.Message is null)
                        {
                            msg.TaskCompletionSource.TrySetException(new Exception("Cannot process message without a message to send"));
                            continue;
                        }

                        if (queues.TryGetValue(msg.QueueUrl, out var messages))
                        {
                            messages.Add(msg);
                            if (messages.Count == 10) // This is the limit for the AWS request
                            {
                                queues.Remove(msg.QueueUrl);
                                _ = ProcessMessages(msg.QueueUrl, messages);
                            }
                        }
                        else
                        {
                            queues[msg.QueueUrl] = new List<SendSqsMessageCommand>
                            {
                                msg
                            };
                        }
                    }
                    else
                    {
                        if (queues.Count > 0)
                        {
                            // process all messages
                            foreach (var set in queues)
                                _ = ProcessMessages(set.Key, set.Value);
                        }

                        await _messageReader.WaitToReadAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

        private async Task ProcessMessages(string queueUrl, List<SendSqsMessageCommand> messages)
        {
            try
            {
                var request = new SendMessageBatchRequest(queueUrl, messages.Select(x => x.Message).ToList());
                var options = _optionsMonitor.CurrentValue;
                var config = new AmazonSQSConfig
                {
                    ServiceURL = options.ServiceURL
                };

                if (options.RegionEndpoint != null)
                    config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.RegionEndpoint);

                using var client = new AmazonSQSClient(config);

                var response = await client.SendMessageBatchAsync(request);

                if (response.Failed.Count > 0)
                    throw new Exception("One or more messages failed to send");

                foreach (var msg in messages)
                    msg.TaskCompletionSource.TrySetResult(true);
            }
            catch (Exception e)
            {
                foreach (var msg in messages)
                    msg.TaskCompletionSource.TrySetException(e);
            }
        }
    }
}