using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;
using OpenMessage.Extensions;
using OpenMessage.Serialisation;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsConsumer<T> : ISqsConsumer<T>
    {
        private readonly IOptionsMonitor<SQSConsumerOptions> _options;
        private readonly IDeserializationProvider _deserializationProvider;
        private IAmazonSQS _client;
        private SQSConsumerOptions _currentConsumerOptions;
        private Func<SqsMessage<T>, Task> _acknowledgementAction;
        private readonly List<SqsMessage<T>> _emptyList = new List<SqsMessage<T>>(0);

        public SqsConsumer(IOptionsMonitor<SQSConsumerOptions> options, IDeserializationProvider deserializationProvider)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _deserializationProvider = deserializationProvider ?? throw new ArgumentNullException(nameof(deserializationProvider));
        }

        public void Initialize(string consumerId)
        {
            while (true)
            {
                try
                {
                    _currentConsumerOptions = _options.Get(consumerId);
                    var config = new AmazonSQSConfig
                    {
                        ServiceURL = _currentConsumerOptions.ServiceURL
                    };

                    if (!string.IsNullOrEmpty(_currentConsumerOptions.RegionEndpoint))
                        config.RegionEndpoint = RegionEndpoint.GetBySystemName(_currentConsumerOptions.RegionEndpoint);


                    _client = new AmazonSQSClient(config);
                    _acknowledgementAction = msg => _client.DeleteMessageAsync(msg.QueueUrl, msg.ReceiptHandle);

                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }
        }

        public async Task<List<SqsMessage<T>>> ConsumeAsync()
        {
            _currentConsumerOptions.Must().NotBeNull("Consumer has not been initialized. Please call Initialize with the configured consumer id.");
            _client.Must().NotBeNull("Consumer has not been initialized. Please call Initialize with the configured consumer id.");

            var request = new ReceiveMessageRequest
            {
                QueueUrl = _currentConsumerOptions.QueueUrl,
                MaxNumberOfMessages = _currentConsumerOptions.MaxNumberOfMessages,
                WaitTimeSeconds = _currentConsumerOptions.WaitTimeSeconds
            };

            if (_currentConsumerOptions.VisibilityTimeout.HasValue)
            {
                request.VisibilityTimeout = _currentConsumerOptions.VisibilityTimeout.Value;
            }

            var response = await _client.ReceiveMessageAsync(request);
            if (response.HttpStatusCode != HttpStatusCode.OK || (response.Messages?.Count ?? 0) == 0)
                return _emptyList;

            var result = new List<SqsMessage<T>>(response.Messages.Count);
            foreach (var message in response.Messages)
            {
                var properties = new List<KeyValuePair<string, string>>(message.Attributes.Count + message.MessageAttributes.Count);

                foreach (var attribute in message.Attributes)
                    properties.Add(new KeyValuePair<string, string>(attribute.Key, attribute.Value));

                foreach (var msgAttribute in message.MessageAttributes)
                    properties.Add(new KeyValuePair<string, string>(msgAttribute.Key, msgAttribute.Value.StringValue));

                var contentType = ContentTypes.Json;
                if (message.MessageAttributes.TryGetValue(KnownProperties.ContentType, out var cta))
                    contentType = cta.StringValue;

                result.Add(new SqsMessage<T>(_acknowledgementAction)
                {
                    Id = message.MessageId,
                    Properties = properties,
                    ReceiptHandle = message.ReceiptHandle,
                    QueueUrl = _currentConsumerOptions.QueueUrl,
                    Value = _deserializationProvider.From<T>(message.Body, contentType)
                });
            }

            return result;
        }
    }
}