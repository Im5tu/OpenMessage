using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;
using OpenMessage.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsConsumer<T> : ISqsConsumer<T>
    {
        private static readonly List<string> RequestAttributes = new List<string>(1) { "All" };
        private static readonly string MisconfiguredConsumerMessage = "Consumer has not been initialized. Please call Initialize with the configured consumer id.";
        private readonly IDeserializationProvider _deserializationProvider;
        private readonly ILogger<SqsConsumer<T>> _logger;
        private readonly List<SqsMessage<T>> _emptyList = new List<SqsMessage<T>>(0);

        private readonly IOptionsMonitor<SQSConsumerOptions> _options;
        private Func<SqsMessage<T>, Task> _acknowledgementAction;
        private IAmazonSQS _client;
        private SQSConsumerOptions _currentConsumerOptions;

        public SqsConsumer(IOptionsMonitor<SQSConsumerOptions> options, IDeserializationProvider deserializationProvider, ILogger<SqsConsumer<T>> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _deserializationProvider = deserializationProvider ?? throw new ArgumentNullException(nameof(deserializationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<SqsMessage<T>>> ConsumeAsync(CancellationToken cancellationToken)
        {
            if (_currentConsumerOptions is null || _client is null)
                Throw.Exception(MisconfiguredConsumerMessage);

            var request = new ReceiveMessageRequest
            {
                QueueUrl = _currentConsumerOptions.QueueUrl,
                MaxNumberOfMessages = _currentConsumerOptions.MaxNumberOfMessages,
                WaitTimeSeconds = _currentConsumerOptions.WaitTimeSeconds,
                AttributeNames = RequestAttributes,
                MessageAttributeNames = RequestAttributes
            };

            if (_currentConsumerOptions.VisibilityTimeout.HasValue)
                request.VisibilityTimeout = _currentConsumerOptions.VisibilityTimeout.Value;

            var response = await _client.ReceiveMessageAsync(request, cancellationToken);

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

                var messageType = default(string);
                if (message.MessageAttributes.TryGetValue(KnownProperties.ValueTypeName, out var vtn))
                    messageType = vtn.StringValue;

                result.Add(new SqsMessage<T>(_acknowledgementAction)
                {
                    Id = message.MessageId,
                    Properties = properties,
                    ReceiptHandle = message.ReceiptHandle,
                    QueueUrl = _currentConsumerOptions.QueueUrl,
                    Value = _deserializationProvider.From<T>(message.Body, contentType, messageType)
                });
            }

            return result;
        }

        public void Initialize(string consumerId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    _currentConsumerOptions = _options.Get(consumerId);

                    var config = new AmazonSQSConfig
                    {
                        ServiceURL = _currentConsumerOptions.ServiceURL
                    };

                    if (!string.IsNullOrEmpty(_currentConsumerOptions.RegionEndpoint))
                        config.RegionEndpoint = RegionEndpoint.GetBySystemName(_currentConsumerOptions.RegionEndpoint);

                    _currentConsumerOptions.AwsConsumerConfiguration?.Invoke(config);
                    _acknowledgementAction = msg => _client.DeleteMessageAsync(msg.QueueUrl, msg.ReceiptHandle);
                    _client = new AmazonSQSClient(config);

                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
        }
    }
}