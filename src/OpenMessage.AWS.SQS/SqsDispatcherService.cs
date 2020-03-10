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
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsDispatcherService : BackgroundService
    {
        private readonly ChannelReader<SendMessage> _messageReader;
        private readonly IOptionsMonitor<SQSDispatcherOptions> _optionsMonitor;
        private readonly ILogger<SqsDispatcherService> _logger;

        public SqsDispatcherService(ChannelReader<SendMessage> messageReader, IOptionsMonitor<SQSDispatcherOptions> optionsMonitor, ILogger<SqsDispatcherService> logger)
        {
            _messageReader = messageReader ?? throw new ArgumentNullException(nameof(messageReader));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queues = new Dictionary<string, List<SendMessage>>(StringComparer.Ordinal);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_messageReader.TryRead(out var msg))
                    {
                        if (queues.TryGetValue(msg.QueueUrl, out var messages))
                        {
                            messages.Add(msg);
                            if (messages.Count == 10)
                            {
                                queues.Remove(msg.QueueUrl);
                                _ = ProcessMessages(msg.QueueUrl, messages);
                            }
                        }
                        else
                        {
                            queues[msg.QueueUrl] = new List<SendMessage>
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

        private async Task ProcessMessages(string queueUrl, List<SendMessage> messages)
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
                    throw new Exception("Well balls");

                foreach (var msg in messages)
                    msg.TCS.TrySetResult(true);
            }
            catch (Exception e)
            {
                foreach (var msg in messages)
                    msg.TCS.TrySetException(e);
            }
        }

        // private void ThrowExceptionFromHttpResponse(SendMessageResponse response)
        // {
        //     throw new Exception($"Failed to send the message to SQS. Type: '{TypeCache<T>.FriendlyName}' Queue Url: '{_queueUrl ?? "<NULL>"}' Status Code: '{response.HttpStatusCode}'.");
        // }
    }

    internal class SendMessage
    {
        internal string QueueUrl { get; set; }
        internal SendMessageBatchRequestEntry Message { get; set; }
        internal TaskCompletionSource<bool> TCS { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}